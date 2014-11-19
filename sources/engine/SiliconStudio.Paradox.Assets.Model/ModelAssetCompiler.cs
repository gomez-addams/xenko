﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Assets.Effect;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Importer.Common;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Model
{
    public class ModelAssetCompiler : AssetCompilerBase<ModelAsset>
    {
        protected override void Compile(AssetCompilerContext context, string urlInStorage, UFile assetAbsolutePath, ModelAsset asset, AssetCompilerResult result)
        {
            if (asset.Source == null)
            {
                result.Error("Source cannot be null for Texture Asset [{0}]", asset);
                return;
            }
            
            // Get absolute path of asset source on disk
            var assetDirectory = assetAbsolutePath.GetParent();
            var assetSource = UPath.Combine(assetDirectory, asset.Source);

            var allow32BitIndex = context.GetGraphicsProfile() >= GraphicsProfile.Level_9_2;
            var allowUnsignedBlendIndices = context.GetGraphicsPlatform() != GraphicsPlatform.OpenGLES;
            var extension = asset.Source.GetFileExtension();

            // compute material and lighting configuration dictionaries here because some null reference can occur
            var materials = new Dictionary<string, Tuple<Guid, string>>();
            foreach (var meshParam in asset.MeshParameters)
            {
                if (meshParam.Value.Material != null)
                    materials.Add(meshParam.Key, new Tuple<Guid, string>(meshParam.Value.Material.Id, meshParam.Value.Material.Location));
            }

            // TODO: temporary while the assets are modified
            foreach (var meshParam in asset.MeshParameters)
            {
                meshParam.Value.Parameters.Set(LightingKeys.CastShadows, meshParam.Value.CastShadows);
                meshParam.Value.Parameters.Set(LightingKeys.ReceiveShadows, meshParam.Value.ReceiveShadows);
                meshParam.Value.Parameters.Set(RenderingParameters.RenderLayer, meshParam.Value.Layer);
                //if (meshParam.Value.LightingParameters != null)
                //    meshParam.Value.Parameters.Set(LightingKeys.LightingConfigurations, new ContentReference<LightingConfigurationsSetData>(meshParam.Value.LightingParameters.Id, meshParam.Value.LightingParameters.Location));
            }

            if (ImportFbxCommand.IsSupportingExtensions(extension))
            {
                result.BuildSteps = new ListBuildStep
                    {
                        new ImportFbxCommand
                            {
                                SourcePath = assetSource,
                                Location = urlInStorage,
                                Allow32BitIndex = allow32BitIndex,
                                AllowUnsignedBlendIndices = allowUnsignedBlendIndices,
                                Compact = asset.Compact,
                                PreservedNodes = asset.PreservedNodes,
                                // Transform AssetReference to Tuple<Guid,UFile> as AssetReference is not working
                                Materials = materials,
                                Parameters = asset.MeshParameters.ToDictionary(pair => pair.Key, pair => pair.Value.Parameters),
                                ViewDirectionForTransparentZSort = asset.ViewDirectionForTransparentZSort.HasValue ? asset.ViewDirectionForTransparentZSort.Value : -Vector3.UnitZ,
                            },
                        new WaitBuildStep(),
                    };
            }
            else if (ImportAssimpCommand.IsSupportingExtensions(extension))
            {
                result.BuildSteps = new ListBuildStep
                    {
                        new ImportAssimpCommand
                            {
                                SourcePath = assetSource,
                                Location = urlInStorage,
                                Allow32BitIndex = allow32BitIndex,
                                AllowUnsignedBlendIndices = allowUnsignedBlendIndices,
                                Compact = asset.Compact,
                                PreservedNodes = asset.PreservedNodes,
                                // Transform AssetReference to Tuple<Guid,UFile> as AssetReference is not working
                                Materials = materials,
                                Parameters = asset.MeshParameters.ToDictionary(pair => pair.Key, pair => pair.Value.Parameters),
                            },
                        new WaitBuildStep(),
                    };
            }
            else
            {
                result.Error("No importer found for model extension '{0}. The model '{1}' can't be imported.", extension, assetSource);
            }
        }
    }

    
}