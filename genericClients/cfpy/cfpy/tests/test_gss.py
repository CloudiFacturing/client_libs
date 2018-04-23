"""Ugly but working hard-coded test script for the GSS client"""
import os
import filecmp

import cfpy as cf

auth_url = "https://api.hetcomp.org/authManager/AuthManager?wsdl"
gss_url = "https://api.hetcomp.org/gss-0.1/FileUtilities?wsdl"
username = "???"
project = 'cloudifacturing'
password = "???"

print("Obtaining session token ...")
auth = cf.AuthClient(auth_url)
session_token = auth.get_session_token(username, project, password)

print('\n### Tests on IT4I cluster storage ###')
print("Querying resource information for it4i_anselm://home ...")
gss = cf.GssClient(gss_url)
res_info = gss.get_resource_information('it4i_anselm://home', session_token)
print(res_info)

print("Listing files in swift://caxman/sintef ...")
print(gss.list_files_minimal('it4i_anselm://home', session_token))

print("Uploading a file ...")
try:
    gss_ID = gss.upload('it4i_anselm://home/test_gss.py',
                        session_token, 'test_gss.py')
    print("-> Uploaded file is {}".format(gss_ID))
except AttributeError:
    print("File seems to exist")
    gss_ID = 'it4i_anselm://home/test_gss.py'

print("Downloading the same file ...")
gss.download_to_file(gss_ID, session_token, 'test_gss_downloaded.py')

assert(filecmp.cmp('test_gss.py', 'test_gss_downloaded.py'))

print("Updating the file ...")
gss.update(gss_ID, session_token, 'test_gss.py')

print("Deleting uploaded file ...")
result = gss.delete(gss_ID, session_token)

print("Deleting downloaded file ...")
os.remove('./test_gss_downloaded.py')