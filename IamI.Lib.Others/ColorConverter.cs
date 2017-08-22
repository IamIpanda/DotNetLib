using System.Runtime.InteropServices;

namespace IamI.Lib.Others
{
    public static class ColorConverter
    {
        /// <summary>
        /// 将 HSV/HSB 变换为 RGB 的计算。
        /// </summary>
        /// <param name="h">色相值（Hue）。0-360之间，以角度计数的角度值。</param>
        /// <param name="s">饱和度（Saturation）。0-100之间的百分比。</param>
        /// <param name="v">深度（Value）。0-100之间的百分比。又称饱和度（Brightness）</param>
        /// <returns>R, G, B 值</returns>
        public static (float, float, float) ConvertHSVToRGB(float h, float s, float v)
        {
            var hi = (int) (h / 60F) % 6;
            var f = h / 60 - hi;
            var p = v * (1 - s);
            var q = v * (1 - f * s);
            var t = v * (1 - (1 - f) * s);
            switch (hi)
            {
                case 0: return (v * 255, t * 255, p * 255);
                case 1: return (q * 255, v * 255, p * 255);
                case 2: return (p * 255, v * 255, t * 255);
                case 3: return (p * 255, q * 255, v * 255);
                case 4: return (t * 255, p * 255, v * 255);
                case 5: return (v * 255, p * 255, q * 255);
                default: return(h * 255, s * 255, v * 255);
            }
        }


        /// <summary>
        /// 将 HSL 变换到 RGB 的计算。
        /// </summary>
        /// <param name="h">色相值（Hue）。0-360之间，以角度计数的角度值。</param>
        /// <param name="s">饱和度（Saturation）。0-100之间的百分比。</param>
        /// <param name="l">亮度（Brightness）。0-100之间的百分比。</param>
        /// <returns>R, G, B 值</returns>
        public static (float, float, float) ConvertHSLToRGB(float h, float s, float l)
        {
            h /= 360F;
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            return (ConvertHueToRGB(p, q, h + 1 / 3F) * 255, ConvertHueToRGB(p, q, h) * 255,
                ConvertHueToRGB(p, q, h - 1 / 3F) * 255);

            float ConvertHueToRGB(float inner_p, float inner_q, float inner_t)
            {
                if (inner_t < 0) inner_t += 1;
                if (inner_t > 1) inner_t -= 1;
                if (inner_t < 1 / 6F) return inner_p + (inner_q - inner_p) * 6 * inner_t;
                if (inner_t < 1 / 2F) return inner_q;
                if (inner_t < 2 / 3F) return inner_p + (inner_q - inner_p) * (2 / 3F - inner_t) * 6;
                return inner_p;
            }
        }

        /// <summary>
        /// 根据色调转换颜色
        /// </summary>
        /// <param name="r">要转换的颜色的 R</param>
        /// <param name="g">要转换的颜色的 G</param>
        /// <param name="b">要转换的颜色的 B</param>
        /// <param name="tone_r">色调的 R</param>
        /// <param name="tone_g">色调的 G</param>
        /// <param name="tone_b">色调的 B</param>
        /// <param name="tone_a">色调的 A</param>
        /// <returns>R, G, B 值</returns>
        public static (int, int, int) ChangeTone(int r, int g, int b, float tone_r, float tone_g, float tone_b, float tone_a)
        {
            var grayfull = (r * 38 + g * 75 + b * 15) >> 7;
            var result_r = (int) (tone_r + r + (grayfull - r) * tone_a / 256F);
            var result_g = (int) (tone_g + g + (grayfull - g) * tone_a / 256F);
            var result_b = (int) (tone_b + b + (grayfull - b) * tone_a / 256F);
            if (result_r > 255) result_r = 255;
            if (result_g > 255) result_g = 255;
            if (result_b > 255) result_b = 255;
            if (result_r < 0) result_r = 0;
            if (result_g < 0) result_g = 0;
            if (result_b < 0) result_b = 0;
            return (result_r, result_g, result_b);
        }
    }


}