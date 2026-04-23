import requests
import json
import os
import base64

AK = "YOUR_VOLCENGINE_ACCESS_KEY"
SK = "YOUR_VOLCENGINE_SECRET_KEY"
MODEL = "doubao-seedream-4-0-250828"
API_URL = "https://ark.cn-beijing.volces.com/api/v3/images/generations"

def decode_sk():
    try:
        decoded = base64.b64decode(SK).decode('utf-8')
        return decoded
    except:
        return SK

def generate_image(prompt, output_path):
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {AK}"
    }

    data = {
        "model": MODEL,
        "prompt": prompt,
        "sequential_image_generation": "disabled",
        "response_format": "url",
        "size": "1024x1024",
        "stream": False,
        "watermark": True
    }

    try:
        print(f"正在生成图片: {prompt[:50]}...")
        response = requests.post(API_URL, headers=headers, json=data, timeout=30)
        print(f"响应状态码: {response.status_code}")
        print(f"响应内容: {response.text}")

        if response.status_code == 200:
            result = response.json()
            if 'data' in result and len(result['data']) > 0:
                image_url = result['data'][0].get('url')
                if image_url:
                    print(f"图片生成成功: {image_url}")
                    return image_url
            print("响应中没有图片URL")
            return None
        else:
            print(f"请求失败: {response.status_code}")
            return None

    except requests.exceptions.Timeout:
        print("请求超时")
        return None
    except requests.exceptions.RequestException as e:
        print(f"请求异常: {e}")
        return None
    except Exception as e:
        print(f"发生错误: {e}")
        return None

def download_image(url, output_path):
    try:
        print(f"正在下载图片到: {output_path}")
        response = requests.get(url, timeout=30)
        if response.status_code == 200:
            os.makedirs(os.path.dirname(output_path), exist_ok=True)
            with open(output_path, 'wb') as f:
                f.write(response.content)
            print(f"图片保存成功: {output_path}")
            return True
        else:
            print(f"下载失败: {response.status_code}")
            return False
    except Exception as e:
        print(f"下载异常: {e}")
        return False

def main():
    items = [
        {
            "id": "EQ_WEAP_001",
            "name": "铁制长剑",
            "prompt": "铁制长剑，中世纪奇幻RPG风格，游戏装备图标，高清金属质感，居中展示，白色背景",
            "category": "Equipment",
            "path": "Assets/Resources/PackageIcon/Equipment/EQ_WEAP_001.png"
        },
        {
            "id": "EQ_ARMOR_002",
            "name": "守护之板甲胸甲",
            "prompt": "守护之板甲胸甲，矮人铁匠打造的重型板甲，中世纪奇幻RPG风格，游戏装备图标，高清金属质感，居中展示，白色背景",
            "category": "Equipment",
            "path": "Assets/Resources/PackageIcon/Equipment/EQ_ARMOR_002.png"
        },
        {
            "id": "EQ_ACC_003",
            "name": "永燃之戒",
            "prompt": "永燃之戒，镶嵌火焰核心的传奇戒指，中世纪奇幻RPG风格，游戏装备图标，高清金属质感，居中展示，白色背景",
            "category": "Equipment",
            "path": "Assets/Resources/PackageIcon/Equipment/EQ_ACC_003.png"
        }
    ]

    for item in items:
        print(f"\n{'='*60}")
        print(f"正在生成: {item['name']} ({item['id']})")
        print(f"{'='*60}")

        image_url = generate_image(item['prompt'], item['path'])
        if image_url:
            download_image(image_url, item['path'])

if __name__ == "__main__":
    main()
