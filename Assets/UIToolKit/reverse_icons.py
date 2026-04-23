from PIL import Image
import os

def process_icon(input_path, output_path):
    """处理图标：对于白色背景的图标，将黑色变为白色，白色变为黑色"""
    try:
        # 打开图片
        img = Image.open(input_path)
        print(f"  Processing: {os.path.basename(input_path)}")

        # 转换为RGBA
        if img.mode != 'RGBA':
            img = img.convert('RGBA')

        # 获取像素数据
        pixels = img.load()
        width, height = img.size

        # 第一步：分析背景颜色
        # 统计边缘像素的颜色
        edge_pixels = []
        # 检查四周边缘的像素
        for x in range(width):
            edge_pixels.append(pixels[x, 0])  # 顶部边缘
            edge_pixels.append(pixels[x, height-1])  # 底部边缘
        for y in range(1, height-1):
            edge_pixels.append(pixels[0, y])  # 左侧边缘
            edge_pixels.append(pixels[width-1, y])  # 右侧边缘

        # 计算边缘像素的平均RGB值
        total_r = total_g = total_b = 0
        count = 0
        for pixel in edge_pixels:
            r, g, b, a = pixel
            if a > 100:  # 只考虑不透明的像素
                total_r += r
                total_g += g
                total_b += b
                count += 1

        if count > 0:
            avg_r = total_r / count
            avg_g = total_g / count
            avg_b = total_b / count
            print(f"  Background: R={avg_r:.1f}, G={avg_g:.1f}, B={avg_b:.1f}")

            # 判断是否是白色背景（RGB值都接近255）
            is_white_background = (avg_r > 200 and avg_g > 200 and avg_b > 200)
            print(f"  Is white background: {is_white_background}")

            if is_white_background:
                # 第二步：颜色反转 - 黑色变白，白色变黑
                for y in range(height):
                    for x in range(width):
                        r, g, b, a = pixels[x, y]
                        
                        if a > 100:  # 只处理不透明的像素
                            # 计算亮度
                            brightness = (r + g + b) / 3
                            
                            if brightness < 100:  # 黑色或深色，变为白色
                                pixels[x, y] = (255, 255, 255, a)
                            else:  # 白色或浅色，变为黑色
                                pixels[x, y] = (0, 0, 0, a)

                # 第三步：保存
                img.save(output_path, 'PNG')
                print(f"  [OK] Saved: {os.path.basename(output_path)}")
                return True
            else:
                print(f"  [SKIP] Not white background")
                return False
        else:
            print(f"  [WARN] No edge pixels found")
            return False

    except Exception as e:
        print(f"  [FAIL] {input_path}: {e}")
        return False

def main():
    # 图标目录
    icons_dir = r"d:\utest\FinalRPG\Assets\UIToolKit"

    # 要处理的图标列表（除了Bg.png和package.png）
    icons = [
        ("arrow.png", "arrow_reversed.png"),
        ("cycle.png", "cycle_reversed.png"),
        ("exit.png", "exit_reversed.png"),
        ("lock.png", "lock_reversed.png"),
        ("trigle.png", "trigle_reversed.png")
    ]

    print("Processing icons (reversing colors for white background icons)...")
    print("=" * 60)

    for icon_name, output_name in icons:
        input_path = os.path.join(icons_dir, icon_name)
        output_path = os.path.join(icons_dir, output_name)

        if os.path.exists(input_path):
            process_icon(input_path, output_path)
        else:
            print(f"  [WARN] File not found: {input_path}")

    print("=" * 60)
    print("Done!")

if __name__ == "__main__":
    main()
