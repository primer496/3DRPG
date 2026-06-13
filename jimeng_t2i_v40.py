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
    image_data = base64.b64decode(base64_str)
    with open(output_path, 'wb') as f:
        f.write(image_data)
    print(f'图片已保存到: {output_path}')

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
        
        try:
            response_json = r.json()
            
            if 'data' in response_json:
                data = response_json['data']
                
                if isinstance(data, list) and len(data) > 0:
                    first_result = data[0]
                    
                    if 'image_base64' in first_result or 'image' in first_result:
                        image_base64 = first_result.get('image_base64', first_result.get('image', ''))
                        
                        timestamp = datetime.datetime.now().strftime('%Y%m%d_%H%M%S')
                        output_filename = f'jimeng_output_{timestamp}.png'
                        
                        save_base64_image(image_base64, output_filename)
                    else:
                        print('响应中未找到图片数据')
                else:
                    print('响应数据为空或格式不正确')
            else:
                print('响应中未找到data字段')
                
        except json.JSONDecodeError as e:
            print(f'JSON解析错误: {e}')

if __name__ == "__main__":
    access_key = 'AKLTOWMxMWZiOTQxNzVkNGQ2ZGEyNWY5NGZhZTAzYjVkMmY'
    secret_key = 'WWpGbE9EVXpNVE5pTUdGaE5HUmhaamd4T0RJMk9URTNNR0UzTWpFNFpUZw=='
    
    query_params = {
        'Action': 'CVProcess',
        'Version': '2022-08-31',
    }
    
    formatted_query = formatQuery(query_params)
    
    prompt_text = input("请输入生成图片的描述文字: ")
    
    body_params = {
        "req_key": "jimeng_t2i_v40",
        "prompt": prompt_text,
        "width": 1024,
        "height": 1024,
        "seed": -1,
        "scale": 3.5,
        "steps": 30,
        "ddim_steps": 20,
        "use_sr": True,
        "return_url": False
    }
    
    formatted_body = json.dumps(body_params)
    
    signV4Request(access_key, secret_key, service, formatted_query, formatted_body)
