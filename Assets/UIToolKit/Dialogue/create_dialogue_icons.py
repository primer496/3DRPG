from PIL import Image, ImageDraw
import os

def create_icon(filename, width, height, bg_color, icon_color):
    # 创建一个新的图像
    image = Image.new('RGBA', (width, height), bg_color)
    draw = ImageDraw.Draw(image)
    
    # 绘制简单的图标
    if filename == 'close.png':
        # 绘制关闭图标
        draw.line([(width*0.3, width*0.3), (width*0.7, width*0.7)], fill=icon_color, width=3)
        draw.line([(width*0.7, width*0.3), (width*0.3, width*0.7)], fill=icon_color, width=3)
    elif filename == 'arrow_left.png':
        # 绘制左箭头
        draw.polygon([(width*0.7, width*0.2), (width*0.3, width*0.5), (width*0.7, width*0.8)], fill=icon_color)
    elif filename == 'arrow_right.png':
        # 绘制右箭头
        draw.polygon([(width*0.3, width*0.2), (width*0.7, width*0.5), (width*0.3, width*0.8)], fill=icon_color)
    
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
        ('close.png', 24, 24),
        ('arrow_left.png', 24, 24),
        ('arrow_right.png', 24, 24)
    ]
    
    # 确保目录存在
    os.makedirs(os.path.dirname(os.path.abspath(__file__)), exist_ok=True)
    
    # 创建图标
    for filename, width, height in icons:
        create_icon(filename, width, height, light_black, white)
