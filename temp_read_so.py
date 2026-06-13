import os, re, json

def read_asset(path):
    for enc in ['utf-8', 'utf-8-sig', 'latin-1']:
        try:
            with open(path, 'r', encoding=enc) as f:
                return f.read()
        except:
            pass
    return ""

item_dir = r"d:\utest\FinalRPG\Assets\Resources\GameConfigs\PackageModel"
quest_dir = r"d:\utest\FinalRPG\Assets\Resources\GameConfigs\Quest"

print("=== ItemData ===")
for fn in sorted(os.listdir(item_dir)):
    if not fn.endswith('.asset'): continue
    content = read_asset(os.path.join(item_dir, fn))
    def g(pattern): 
        m = re.search(pattern, content)
        return m.group(1) if m else ''
    itemID    = g(r'itemID:\s*"([^"]*)"')
    itemName  = g(r'_itemName:\s*"([^"]*)"')
    desc      = g(r'_description:\s*"([^"]*)"')
    iconPath  = g(r'iconPath:\s*"([^"]*)"')
    category  = g(r'category:\s*(\d+)')
    rarity    = g(r'rarity:\s*(\d+)')
    isStack   = g(r'isStackable:\s*(\d+)')
    maxStack  = g(r'maxStack:\s*(\d+)')
    print(f"{fn[:-6]}|{itemID}|{itemName}|{desc}|{iconPath}|{category}|{rarity}|{isStack}|{maxStack}")

print()
print("=== QuestData ===")
for fn in sorted(os.listdir(quest_dir)):
    if not fn.endswith('.asset'): continue
    content = read_asset(os.path.join(quest_dir, fn))
    print(f"--- {fn[:-6]} ---")
    print(content)
    print()
