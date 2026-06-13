import os
import re

files = ['generate_item_icons.py', 'generate_jimeng_images.py']
patterns = [
    (r'AKLTOWMxMWZiOTQxNzVkNGQ2ZGEyNWY5NGZhZTAzYjVkMmY', 'YOUR_VOLCENGINE_ACCESS_KEY'),
    (r'WWpGbE9EVXpNVE5pTUdGaE5HUmhaamd4T0RJMk9URTNNR0UzTWpFNFpUZw==', 'YOUR_VOLCENGINE_SECRET_KEY'),
]

for fname in files:
    if os.path.exists(fname):
        with open(fname, 'r', encoding='utf-8') as f:
            content = f.read()
        for pat, rep in patterns:
            content = content.replace(pat, rep)
        with open(fname, 'w', encoding='utf-8') as f:
            f.write(content)
        print(f'Fixed: {fname}')
