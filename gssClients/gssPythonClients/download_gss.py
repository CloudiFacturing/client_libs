#!/bin/env python

# Copyright STIFTELSEN SINTEF 2016

import suds
import urllib2
import sys
if len(sys.argv) < 4:
    print ("Usage:")
    print ("\t %s gss-url outputfilename token" % sys.argv[0])
    exit()

# get url: 
url = sys.argv[1]
outputfileName = sys.argv[2]
sessionToken = sys.argv[3]

wsdlLocation = "https://api.caxman.eu/sintef/infrastructure/gss-0.1/FileUtilities?wsdl"

client = suds.client.Client(wsdlLocation)
resourceInformation = client.service.getResourceInformation(url, sessionToken)
readDescription = resourceInformation.readDescription

if readDescription.supported:
    headers = {}
    headers[readDescription.sessionTokenField] = sessionToken
    if hasattr(readDescription, "headers"):
        for headerField in readDescription.headers:
            headers[headerField.key] = headerField.value

    with open(outputfileName, "wb") as outputFile:
        request = urllib2.Request(url = readDescription.url, headers=headers)           
        
        result = urllib2.urlopen(request)
        while True:
            buffer = result.read()
            if not buffer:
                break
            outputFile.write(buffer)
else:
    print "The given gss_url does not support read/download."
