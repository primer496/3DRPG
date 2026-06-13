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
    datestamp = t.strftime('%Y%m%d')  # Date w/o time, used in credential scope
    canonical_uri = '/'
    canonical_querystring = req_query
    signed_headers = 'content-type;host;x-content-sha256;x-date'
    payload_hash = hashlib.sha256(req_body.encode('utf-8')).hexdigest()
    content_type = 'application/json'
    canonical_headers = 'content-type:' + content_type + '\n' + 'host:' + host + '\n' + 'x-content-sha256:' + payload_hash + '\n' + 'x-date:' + current_date + '\n'
    canonical_request = method + '\n' + canonical_uri + '\n' + canonical_querystring + '\n' + canonical_headers + '\n' + signed_headers + '\n' + payload_hash
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

    # ************* SEND THE REQUEST *************
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
        # 使用 replace 方法将 \u0026 替换为 &
        resp_str = r.text.replace("\\u0026", "&")
        print(f'Response body: {resp_str}\n')
        return r.json()

def generate_icon(prompt, output_file):
    # 请求凭证，从访问控制申请
    access_key = 'AKLTOWMxMWZiOTQxNzVkNGQ2ZGEyNWY5NGZhZTAzYjVkMmY'
    secret_key = 'WWpGbE9EVXpNVE5pTUdGaE5HUmhaamd4T0RJMk9URTNNR0UzTWpFNFpUZw=='

    # 请求Query，按照接口文档中填入即可
    query_params = {
        'Action': 'CVProcess',
        'Version': '2022-08-31',
    }
    formatted_query = formatQuery(query_params)

    # 请求Body，按照接口文档中填入即可
    body_params = {
        "req_key": "Text2Image",
        "prompt": prompt,
        "size": "512x512",
        "model": "jimeng_t2i_v40"
    }
    formatted_body = json.dumps(body_params)
    
    response = signV4Request(access_key, secret_key, service,
                  formatted_query, formatted_body)
    
    # 保存生成的图片
    if 'data' in response and 'image' in response['data']:
        image_data = response['data']['image']
        with open(output_file, 'wb') as f:
            f.write(base64.b64decode(image_data))
        print(f"Image saved to {output_file}")
    else:
        print("Failed to generate image")

if __name__ == "__main__":
    # 生成所需的图标
    icons = [
        ("arrow icon, white on light black background, simple, minimalist", "arrow.png"),
        ("refresh icon, white on light black background, simple, minimalist", "cycle_reversed.png"),
        ("lock icon, white on light black background, simple, minimalist", "lock.png"),
        ("dropdown arrow icon, white on light black background, simple, minimalist", "trigle_reversed.png"),
        ("inventory background, dark gray, simple, minimalist", "Bg.png")
    ]
    
    # 确保目录存在
    os.makedirs(os.path.dirname(os.path.abspath(__file__)), exist_ok=True)
    
    # 生成图标
    for prompt, filename in icons:
        output_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), filename)
        print(f"Generating {filename}...")
        generate_icon(prompt, output_path)
