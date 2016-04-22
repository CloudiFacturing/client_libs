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
uploadDescription = resourceInformation.createDescription
if not uploadDescription.supported:
    uploadDescription = resourceInformation.updateDescription

if uploadDescription.supported:
    print uploadDescription
    headers = {}
    headers[uploadDescription.sessionTokenField] = sessionToken
    headers["Content-Length"]= "%d" % os.stat(filenameIn).st_size
    if hasattr(uploadDescription, "headers"):
        for headerField in uploadDescription.headers:
            headers[headerField.key] = headerField.value
    
    with open(filenameIn, "r") as inputFile:
        tmpUrl = uploadDescription.url
        request = urllib2.Request(url = tmpUrl, data=inputFile, headers=headers)                 
        request.get_method = lambda: uploadDescription.httpMethod;
        result = urllib2.urlopen(request)
        #print "HTML response: " + str(result.getcode())
        
    if resourceInformation.queryForName:
        #print "\n"
        print result.info()["filename"]
    else:
        print url
else:
    print "Create is not a supported operation for this file"
