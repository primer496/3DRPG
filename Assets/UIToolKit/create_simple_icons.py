from PIL import Image, ImageDraw
import os

def create_icon(filename, width, height, bg_color, icon_color):
    # 创建一个新的图像
    image = Image.new('RGBA', (width, height), bg_color)
    draw = ImageDraw.Draw(image)
    
    # 绘制简单的图标
    if filename == 'arrow.png':
        # 绘制左箭头
        draw.polygon([(width*0.7, height*0.2), (width*0.3, height*0.5), (width*0.7, height*0.8)], fill=icon_color)
    elif filename == 'cycle_reversed.png':
        # 绘制刷新图标
        draw.ellipse([(width*0.2, width*0.2), (width*0.8, width*0.8)], outline=icon_color, width=3)
        draw.polygon([(width*0.8, width*0.5), (width*0.6, width*0.3), (width*0.6, width*0.7)], fill=icon_color)
    elif filename == 'lock.png':
        # 绘制锁图标
        draw.rectangle([(width*0.3, width*0.4), (width*0.7, width*0.8)], fill=icon_color)
        draw.ellipse([(width*0.3, width*0.3), (width*0.7, width*0.5)], fill=icon_color)
        draw.rectangle([(width*0.4, width*0.5), (width*0.6, width*0.6)], fill=bg_color)
    elif filename == 'trigle_reversed.png':
        # 绘制下拉箭头
        draw.polygon([(width*0.3, width*0.3), (width*0.7, width*0.3), (width*0.5, width*0.7)], fill=icon_color)
    elif filename == 'Bg.png':
        # 绘制背景
        pass  # 背景已经是纯色
    
    # 保存图像
    output_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), filename)
    image.save(output_path, 'PNG')
    print(f"Created {filename}")

if __name__ == "__main__":
    # 定义颜色
    light_black = (30, 30, 30, 255)  # 浅黑色背景
    white = (255, 255, 255, 255)     # 白色图标
    
    # 生成图标
    icons = [
        ('arrow.png', 48, 48),
        ('cycle_reversed.png', 48, 48),
        ('lock.png', 48, 48),
        ('trigle_reversed.png', 24, 24),
        ('Bg.png', 1024, 1024)
    ]
    
    # 确保目录存在
    os.makedirs(os.path.dirname(os.path.abspath(__file__)), exist_ok=True)
    
    # 创建图标
    for filename, width, height in icons:
        create_icon(filename, width, height, light_black, white)
