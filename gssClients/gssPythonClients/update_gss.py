#!/bin/env python

# Copyright STIFTELSEN SINTEF 2016

import suds
import urllib2
import sys
import os
if len(sys.argv) < 4:
    print ("Usage:")
    print ("\t %s gss-url inputfilename token" % sys.argv[0])
    exit()

# get url: 
url = sys.argv[1]
filenameIn = sys.argv[2]
sessionToken = sys.argv[3]
wsdl_url = "https://api.caxman.eu/sintef/infrastructure/gss-0.1/FileUtilities?wsdl"

client = suds.client.Client(wsdl_url)

resourceInformation = client.service.getResourceInformation(url, sessionToken)
updateDescription = resourceInformation.updateDescription

if updateDescription.supported:
    headers = {}
    headers[updateDescription.sessionTokenField] = sessionToken
    headers["Content-Length"]= "%d" % os.stat(filenameIn).st_size
    if hasattr(updateDescription, "headers"):
        for headerField in updateDescription.headers:
            headers[headerField.key] = headerField.value
    
    with open(filenameIn, "r") as inputFile:
        request = urllib2.Request(url=updateDescription.url, data=inputFile, headers=headers)                      
        request.get_method = lambda: updateDescription.httpMethod;
        result = urllib2.urlopen(request)
        
    if resourceInformation.queryForName:
        print result.info()["filename"]
    else:
        print url
else:
    print "Update is not a supported operation for this file"
