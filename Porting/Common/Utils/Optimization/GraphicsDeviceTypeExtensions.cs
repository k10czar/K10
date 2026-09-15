using UnityEngine.Rendering;

public static class GraphicsDeviceTypeExtensions
{
    /// <summary> Make a prettier string with limited size</summary>
    /// <param name="gfx"></param>
    /// <returns>string with no more than 10 characters</returns>
    public static string Pretty( this GraphicsDeviceType gfx )
    {
#pragma warning disable CS0618
        return gfx switch
        {
            GraphicsDeviceType.OpenGL2            => "OpenGL2",
            GraphicsDeviceType.Direct3D9          => "D3D9",
            GraphicsDeviceType.Direct3D11         => "D3D11",
            GraphicsDeviceType.Direct3D12         => "D3D12",
            GraphicsDeviceType.Null               => "Null",
            GraphicsDeviceType.OpenGLES2          => "GLES2",
            GraphicsDeviceType.OpenGLES3          => "GLES3",
            GraphicsDeviceType.OpenGLCore         => "GLCore",
            GraphicsDeviceType.Metal              => "Metal",
            GraphicsDeviceType.Vulkan             => "Vulkan",
            GraphicsDeviceType.WebGPU             => "WebGPU",
            GraphicsDeviceType.PlayStation3       => "PS3",
            GraphicsDeviceType.PlayStation4       => "PS4",
            GraphicsDeviceType.PlayStation5       => "PS5",
            GraphicsDeviceType.PlayStation5NGGC   => "PS5 NGGC",
            GraphicsDeviceType.PlayStationVita    => "PSVita",
            GraphicsDeviceType.PlayStationMobile  => "PSMobile",
            GraphicsDeviceType.Xbox360            => "Xbox360",
            GraphicsDeviceType.XboxOne            => "XboxOne",
            GraphicsDeviceType.XboxOneD3D12       => "XboxOneD12",
            GraphicsDeviceType.GameCoreXboxOne    => "GCXboxOne",
            GraphicsDeviceType.GameCoreScarlett   => "GCScarlett",
            GraphicsDeviceType.GameCoreXboxSeries => "GCXboxSX",
            GraphicsDeviceType.N3DS               => "3DS",
            GraphicsDeviceType.Switch             => "Switch",
            GraphicsDeviceType.Switch2            => "Switch2",
            _                                     => gfx.ToString(),
        };
#pragma warning restore CS0618
    }
}