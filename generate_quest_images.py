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

def save_base64_image(base64_str, output_path):
    try:
        image_data = base64.b64decode(base64_str)
        
        output_dir = os.path.dirname(output_path)
        if output_dir and not os.path.exists(output_dir):
            os.makedirs(output_dir, exist_ok=True)
            print(f'[INFO] Created directory: {output_dir}')
        
        with open(output_path, 'wb') as f:
            f.write(image_data)
        
        file_size = os.path.getsize(output_path)
        print(f'[OK] Saved: {os.path.basename(output_path)} ({file_size} bytes)')
        return True
    except Exception as e:
        print(f'[ERROR] Save failed: {e}')
        return False

def generate_image(access_key, secret_key, service, req_query, req_body, output_filename):
    if access_key is None or secret_key is None:
        print('[ERROR] No access key available.')
        return False
    
    t = datetime.datetime.utcnow()
    current_date = t.strftime('%Y%m%dT%H%M%SZ')
    datestamp = t.strftime('%Y%m%d')
    
    canonical_uri = '/'
    canonical_querystring = req_query
    signed_headers = 'content-type;host;x-content-sha256;x-date'
    payload_hash = hashlib.sha256(req_body.encode('utf-8')).hexdigest()
    content_type = 'application/json'
    
    canonical_headers = ('content-type:' + content_type + '\n' +
                        'host:' + host + '\n' +
                        'x-content-sha256:' + payload_hash + '\n' +
                        'x-date:' + current_date + '\n')
    
    canonical_request = (method + '\n' + canonical_uri + '\n' +
                        canonical_querystring + '\n' +
                        canonical_headers + '\n' +
                        signed_headers + '\n' +
                        payload_hash)
    
    algorithm = 'HMAC-SHA256'
    credential_scope = datestamp + '/' + region + '/' + service + '/request'
    string_to_sign = (algorithm + '\n' + current_date + '\n' +
                     credential_scope + '\n' +
                     hashlib.sha256(canonical_request.encode('utf-8')).hexdigest())
    
    signing_key = getSignatureKey(secret_key, datestamp, region, service)
    signature = hmac.new(signing_key, string_to_sign.encode('utf-8'),
                        hashlib.sha256).hexdigest()
    
    authorization_header = (algorithm + ' Credential=' + access_key + '/' +
                           credential_scope + ', SignedHeaders=' +
                           signed_headers + ', Signature=' + signature)
    
    headers = {
        'X-Date': current_date,
        'Authorization': authorization_header,
        'X-Content-Sha256': payload_hash,
        'Content-Type': content_type
    }
    
    request_url = endpoint + '?' + canonical_querystring
    
    try:
        print(f'[INFO] Requesting API...')
        r = requests.post(request_url, headers=headers, data=req_body, timeout=120)
        
        print(f'[INFO] HTTP Status: {r.status_code}')
        
        if r.status_code == 200:
            try:
                response_json = r.json()
                
                if response_json.get('code') == 10000 and 'data' in response_json:
                    data = response_json['data']
                    
                    binary_data_list = data.get('binary_data_base64', [])
                    if binary_data_list and len(binary_data_list) > 0:
                        image_base64 = binary_data_list[0]
                        print(f'[INFO] Received image data (length: {len(image_base64)})')
                        
                        return save_base64_image(image_base64, output_filename)
                    else:
                        print('[FAIL] No image data in response')
                        print(f'[DEBUG] Data keys: {list(data.keys())}')
                else:
                    code = response_json.get('code', 'unknown')
                    msg = response_json.get('message', '')[:200]
                    print(f'[FAIL] API error code {code}: {msg}')
            except json.JSONDecodeError as e:
                print(f'[ERROR] JSON parse error: {e}')
                print(f'[DEBUG] Response preview: {r.text[:300]}')
        else:
            print(f'[FAIL] HTTP Error {r.status_code}')
            try:
                err_resp = r.json()
                print(f'[DEBUG] Error details: {str(err_resp)[:400]}')
            except:
                print(f'[DEBUG] Raw response: {r.text[:200]}')
            
    except requests.exceptions.Timeout:
        print('[ERROR] Request timeout (120s)')
    except requests.exceptions.RequestException as e:
        print(f'[ERROR] Network error: {e}')
    except Exception as err:
        print(f'[ERROR] Unexpected error: {err}')
        import traceback
        traceback.print_exc()
    
    return False

if __name__ == "__main__":
    access_key = 'AKLTOWMxMWZiOTQxNzVkNGQ2ZGEyNWY5NGZhZTAzYjVkMmY'
    secret_key = 'WWpGbE9EVXpNVE5pTUdGaE5HUmhaamd4T0RJMk9URTNNR0UzTWpFNFpUZw=='
    
    query_params = {
        'Action': 'CVProcess',
        'Version': '2022-08-31',
    }
    
    formatted_query = formatQuery(query_params)
    
    output_base = os.path.join('D:', os.sep, 'utest', 'FinalRPG', 'Assets', 'UIToolKit', 'Quest', 'Images')
    print(f'[INFO] Output directory: {output_base}')
    
    images_to_generate = [
        {
            'prompt': 'A golden coin stack icon for RPG game UI, pixel art style, simple design on dark background, centered',
            'filename': 'icon_gold.png',
            'width': 1024, 'height': 1024
        },
        {
            'prompt': 'A blue glowing experience orb icon for game UI, magical light effect, pixel art style, centered composition',
            'filename': 'icon_exp.png',
            'width': 1024, 'height': 1024
        },
        {
            'prompt': 'A treasure chest icon with glow effect, fantasy RPG style, pixel art, centered in frame',
            'filename': 'icon_reward.png',
            'width': 1024, 'height': 1024
        },
        {
            'prompt': 'Quest active icon with sword and shield crossed, medieval adventure symbol, clean game UI design, centered',
            'filename': 'icon_quest_active.png',
            'width': 1024, 'height': 1024
        },
        {
            'prompt': 'Completed quest badge icon, golden checkmark on shield, achievement symbol, celebratory design, centered',
            'filename': 'icon_quest_completed.png',
            'width': 1024, 'height': 1024
        },
        {
            'prompt': 'Failed quest icon, broken skull warning symbol, red color scheme, dark fantasy game element, centered',
            'filename': 'icon_quest_failed.png',
            'width': 1024, 'height': 1024
        },
        {
            'prompt': 'Shadow wolf monster portrait, dark creature enemy silhouette, ominous RPG character, detailed artwork, centered',
            'filename': 'monster_wolf.png',
            'width': 1024, 'height': 1024
        },
        {
            'prompt': 'Ancient black dragon boss head portrait, epic menacing dragon face, detailed fantasy artwork, dramatic lighting, centered',
            'filename': 'boss_dragon.png',
            'width': 1024, 'height': 1024
        },
        {
            'prompt': 'Legendary glowing sword weapon, epic fantasy artifact, shiny magical blade with light effects, centered composition',
            'filename': 'item_legendary.png',
            'width': 1024, 'height': 1024
        },
        {
            'prompt': 'Dark parchment scroll background texture for game UI, medieval quest log paper, subtle aged pattern, dark corners, suitable for overlay text',
            'filename': 'bg_panel.png',
            'width': 1024, 'height': 1024
        }
    ]
    
    success_count = 0
    
    for i, img_config in enumerate(images_to_generate):
        print(f'\n{"="*60}')
        print(f'[{i+1}/{len(images_to_generate)}] Generating: {img_config["filename"]}')
        
        body_params = {
            "req_key": "jimeng_t2i_v40",
            "prompt": img_config['prompt'],
            "width": img_config['width'],
            "height": img_config['height'],
            "seed": -1,
            "scale": 3.5,
            "steps": 20,
            "use_sr": True,
            "return_url": False
        }
        
        formatted_body = json.dumps(body_params)
        full_path = os.path.join(output_base, img_config['filename'])
        print(f'[INFO] Target path: {full_path}')
        
        if generate_image(access_key, secret_key, service, formatted_query, formatted_body, full_path):
            success_count += 1
        
        import time
        time.sleep(3)
    
    print(f'\n{"="*60}')
    print(f'RESULT: {success_count}/{len(images_to_generate)} images generated successfully')
    print(f'Output folder: {output_base}')
    
    if success_count > 0:
        print('\n[SUCCESS] All done! Images are ready.')
        print(f'\nGenerated files:')
        for img in images_to_generate:
            filepath = os.path.join(output_base, img['filename'])
            if os.path.exists(filepath):
                size = os.path.getsize(filepath)
                print(f'  ✓ {img["filename"]} ({size:,} bytes)')
            else:
                print(f'  ✗ {img["filename"]} (missing)')
    else:
        print('\n[WARNING] No images were generated.')
