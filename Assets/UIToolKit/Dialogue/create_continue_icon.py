from PIL import Image, ImageDraw
import os

def create_continue_icon(filename, width, height, bg_color, icon_color):
    # 创建一个新的图像
    image = Image.new('RGBA', (width, height), bg_color)
    draw = ImageDraw.Draw(image)
    
    # 绘制继续提示图标（向下的箭头）
    draw.polygon([(width*0.3, width*0.3), (width*0.7, width*0.3), (width*0.5, width*0.7)], fill=icon_color)
    
    # 保存图像
    output_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), filename)
    image.save(output_path, 'PNG')
    print(f"Created {filename}")

if __name__ == "__main__":
    # 定义颜色
    light_black = (30, 30, 30, 255)  # 浅黑色背景
    white = (255, 255, 255, 255)     # 白色图标
    
    # 生成图标
    create_continue_icon('continue.png', 24, 24, light_black, white)
