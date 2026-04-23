import json
import sys
import os
import base64
import datetime
import hashlib
import hmac
import requests


method = 'POST'
host = 'visual.volcengineapi.com'
region = 'cn-north-1'
endpoint = 'https://visual.volcengineapi.com'
service = 'cv'

def sign(key, msg):
    return hmac.new(key, msg.encode('utf-8'), hashlib.sha256).digest()

def getSignatureKey(key, dateStamp, regionName, serviceName):
    kDate = sign(key.encode('utf-8'), dateStamp)
    kRegion = sign(kDate, regionName)
    kService = sign(kRegion, serviceName)
    kSigning = sign(kService, 'request')
    return kSigning

def formatQuery(parameters):
    request_parameters_init = ''
    for key in sorted(parameters):
        request_parameters_init += key + '=' + parameters[key] + '&'
    request_parameters = request_parameters_init[:-1]
    return request_parameters

def signV4Request(access_key, secret_key, service, req_query, req_body):
    if access_key is None or secret_key is None:
        print('No access key is available.')
        sys.exit()

    t = datetime.datetime.utcnow()
    current_date = t.strftime('%Y%m%dT%H%M%SZ')
    datestamp = t.strftime('%Y%m%d')
    canonical_uri = '/'
    canonical_querystring = req_query
    signed_headers = 'content-type;host;x-content-sha256;x-date'
    payload_hash = hashlib.sha256(req_body.encode('utf-8')).hexdigest()
    content_type = 'application/json'
    canonical_headers = 'content-type:' + content_type + '\n' + 'host:' + host + \
        '\n' + 'x-content-sha256:' + payload_hash + \
        '\n' + 'x-date:' + current_date + '\n'
    canonical_request = method + '\n' + canonical_uri + '\n' + canonical_querystring + \
        '\n' + canonical_headers + '\n' + signed_headers + '\n' + payload_hash
    algorithm = 'HMAC-SHA256'
    credential_scope = datestamp + '/' + region + '/' + service + '/' + 'request'
    string_to_sign = algorithm + '\n' + current_date + '\n' + credential_scope + '\n' + hashlib.sha256(
        canonical_request.encode('utf-8')).hexdigest()
    signing_key = getSignatureKey(secret_key, datestamp, region, service)
    signature = hmac.new(signing_key, (string_to_sign).encode(
        'utf-8'), hashlib.sha256).hexdigest()
    authorization_header = algorithm + ' ' + 'Credential=' + access_key + '/' + \
        credential_scope + ', ' + 'SignedHeaders=' + \
        signed_headers + ', ' + 'Signature=' + signature
    headers = {'X-Date': current_date,
               'Authorization': authorization_header,
               'X-Content-Sha256': payload_hash,
               'Content-Type': content_type
               }
    request_url = endpoint + '?' + canonical_querystring
    print('\nBEGIN REQUEST++++++++++++++++++++++++++++++++++++')
    print('Request URL = ' + request_url)
    try:
        r = requests.post(request_url, headers=headers, data=req_body)
    except Exception as err:
        print(f'error occurred: {err}')
        raise
    else:
        print('\nRESPONSE++++++++++++++++++++++++++++++++++++')
        print(f'Response code: {r.status_code}\n')
        resp_str = r.text.replace("\\u0026", "&")
        print(f'Response body: {resp_str}\n')
        return r.json()

def download_image(url, output_path):
    try:
        print(f'正在下载图片到: {output_path}')
        response = requests.get(url, timeout=30)
        if response.status_code == 200:
            os.makedirs(os.path.dirname(output_path), exist_ok=True)
            with open(output_path, 'wb') as f:
                f.write(response.content)
            print(f'图片保存成功: {output_path}')
            return True
        else:
            print(f'下载失败: {response.status_code}')
            return False
    except Exception as e:
        print(f'下载异常: {e}')
        return False

if __name__ == "__main__":
    access_key = 'YOUR_VOLCENGINE_ACCESS_KEY'
    secret_key = 'YOUR_VOLCENGINE_SECRET_KEY'

    query_params = {
        'Action': 'CVProcess',
        'Version': '2022-08-31',
    }
    formatted_query = formatQuery(query_params)

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
        },
        {
            "id": "CONS_HP_001",
            "name": "小型治疗药水",
            "prompt": "小型治疗药水瓶，红色药水，中世纪奇幻RPG风格，游戏消耗品图标，高清质感，居中展示，白色背景",
            "category": "Consumable",
            "path": "Assets/Resources/PackageIcon/Consumable/CONS_HP_001.png"
        },
        {
            "id": "CONS_BUFF_002",
            "name": "高级力量药剂",
            "prompt": "高级力量药剂，金色药水瓶，中世纪奇幻RPG风格，游戏消耗品图标，高清质感，居中展示，白色背景",
            "category": "Consumable",
            "path": "Assets/Resources/PackageIcon/Consumable/CONS_BUFF_002.png"
        },
        {
            "id": "CONS_BUFF_003",
            "name": "全属性增幅秘药",
            "prompt": "全属性增幅秘药，紫色发光药水瓶，中世纪奇幻RPG风格，游戏消耗品图标，高清质感，居中展示，白色背景",
            "category": "Consumable",
            "path": "Assets/Resources/PackageIcon/Consumable/CONS_BUFF_003.png"
        },
        {
            "id": "MAT_ORE_001",
            "name": "铁矿石",
            "prompt": "铁矿石块，灰色金属矿石，中世纪奇幻RPG风格，游戏材料图标，高清质感，居中展示，白色背景",
            "category": "Material",
            "path": "Assets/Resources/PackageIcon/Material/MAT_ORE_001.png"
        },
        {
            "id": "MAT_ENCHANT_002",
            "name": "附魔水晶",
            "prompt": "附魔水晶，蓝色发光宝石，中世纪奇幻RPG风格，游戏材料图标，高清质感，居中展示，白色背景",
            "category": "Material",
            "path": "Assets/Resources/PackageIcon/Material/MAT_ENCHANT_002.png"
        },
        {
            "id": "MAT_LEGEND_003",
            "name": "龙鳞碎片",
            "prompt": "龙鳞碎片，金色龙鳞，中世纪奇幻RPG风格，游戏传说级材料图标，高清质感，居中展示，白色背景",
            "category": "Material",
            "path": "Assets/Resources/PackageIcon/Material/MAT_LEGEND_003.png"
        },
        {
            "id": "QUEST_001",
            "name": "村长的家书",
            "prompt": "村长的家书，泛黄的信封和纸张，中世纪奇幻RPG风格，游戏任务物品图标，高清质感，居中展示，白色背景",
            "category": "QuestItem",
            "path": "Assets/Resources/PackageIcon/QuestItem/QUEST_001.png"
        },
        {
            "id": "QUEST_002",
            "name": "破损的古代石板",
            "prompt": "破损的古代石板，刻有古文字的石碑碎片，中世纪奇幻RPG风格，游戏任务物品图标，高清质感，居中展示，白色背景",
            "category": "QuestItem",
            "path": "Assets/Resources/PackageIcon/QuestItem/QUEST_002.png"
        },
        {
            "id": "QUEST_003",
            "name": "魔王的心脏碎片",
            "prompt": "魔王的心脏碎片，发光红色晶体，中世纪奇幻RPG风格，游戏传说级任务物品图标，高清质感，居中展示，白色背景",
            "category": "QuestItem",
            "path": "Assets/Resources/PackageIcon/QuestItem/QUEST_003.png"
        },
        {
            "id": "ITEM_KEY_001",
            "name": "铜制宝箱钥匙",
            "prompt": "铜制宝箱钥匙，古铜色钥匙，中世纪奇幻RPG风格，游戏道具图标，高清质感，居中展示，白色背景",
            "category": "Item",
            "path": "Assets/Resources/PackageIcon/Item/ITEM_KEY_001.png"
        },
        {
            "id": "ITEM_TP_002",
            "name": "主城传送卷轴",
            "prompt": "主城传送卷轴，魔法卷轴，中世纪奇幻RPG风格，游戏道具图标，高清质感，居中展示，白色背景",
            "category": "Item",
            "path": "Assets/Resources/PackageIcon/Item/ITEM_TP_002.png"
        },
        {
            "id": "ITEM_BAG_003",
            "name": "背包扩容石",
            "prompt": "背包扩容石，发光紫色晶石，中世纪奇幻RPG风格，游戏道具图标，高清质感，居中展示，白色背景",
            "category": "Item",
            "path": "Assets/Resources/PackageIcon/Item/ITEM_BAG_003.png"
        },
        {
            "id": "OTHER_JUNK_001",
            "name": "破损的空酒瓶",
            "prompt": "破损的空酒瓶，绿色玻璃碎片，中世纪奇幻RPG风格，游戏杂物图标，高清质感，居中展示，白色背景",
            "category": "Other",
            "path": "Assets/Resources/PackageIcon/Other/OTHER_JUNK_001.png"
        },
        {
            "id": "OTHER_COLLECT_002",
            "name": "冒险者周年纪念币",
            "prompt": "冒险者周年纪念币，金色硬币，中世纪奇幻RPG风格，游戏收藏品图标，高清质感，居中展示，白色背景",
            "category": "Other",
            "path": "Assets/Resources/PackageIcon/Other/OTHER_COLLECT_002.png"
        },
        {
            "id": "OTHER_FUN_003",
            "name": "恶搞史莱姆面具",
            "prompt": "恶搞史莱姆面具，绿色凝胶面具，中世纪奇幻RPG风格，游戏趣味道具图标，高清质感，居中展示，白色背景",
            "category": "Other",
            "path": "Assets/Resources/PackageIcon/Other/OTHER_FUN_003.png"
        }
    ]

    success_count = 0
    fail_count = 0

    for item in items:
        print(f"\n{'='*60}")
        print(f"正在生成: {item['name']} ({item['id']})")
        print(f"{'='*60}")

        body_params = {
            "req_key": "jimeng_t2i_v40",
            "prompt": item['prompt'],
            "prompt_extend": [],
            "return_url": True,
            "return_fields": [],
            "scale": 1.0,
            "seed": -1,
            "style_meta:": {
                "style_name": ""
            }
        }

        formatted_body = json.dumps(body_params)

        try:
            response = signV4Request(access_key, secret_key, service,
                                   formatted_query, formatted_body)

            if response and 'data' in response:
                image_urls = response['data'].get('image_urls', [])
                if image_urls and len(image_urls) > 0:
                    image_url = image_urls[0]
                    if download_image(image_url, item['path']):
                        success_count += 1
                    else:
                        fail_count += 1
                else:
                    print(f"未获取到图片URL: {response}")
                    fail_count += 1
            else:
                print(f"API响应异常: {response}")
                fail_count += 1

        except Exception as e:
            print(f"生成失败: {e}")
            fail_count += 1

    print(f"\n{'='*60}")
    print(f"图片生成完成！成功: {success_count}, 失败: {fail_count}")
    print(f"{'='*60}")
