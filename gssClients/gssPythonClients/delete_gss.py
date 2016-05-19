#!/bin/env python

# Copyright STIFTELSEN SINTEF 2016

import suds
import urllib2
import sys
if len(sys.argv) < 3:
    print ("Usage:")
    print ("\t %s gss-url token" % sys.argv[0])
    exit()

# get url: 
url = sys.argv[1]
sessionToken = sys.argv[2]

wsdl_url = "https://api.caxman.eu/sintef/infrastructure/gss-0.1/FileUtilities?wsdl"
        
client = suds.client.Client(wsdl_url)
resourceInformation = client.service.getResourceInformation(url, sessionToken)
deleteDescription = resourceInformation.deleteDescription

if deleteDescription.supported:    
    headers = {}
    headers[deleteDescription.sessionTokenField] = sessionToken
    if hasattr(deleteDescription, "headers"):
        for headerField in deleteDescription.headers:
            headers[headerField.key] = headerField.value
    
    request = urllib2.Request(url = deleteDescription.url, headers=headers)
    request.get_method = lambda: deleteDescription.httpMethod
    
    result = urllib2.urlopen(request)
else:
    print "Delete is not supported for the given gss_url"
