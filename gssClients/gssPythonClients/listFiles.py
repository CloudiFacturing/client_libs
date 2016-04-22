#!/bin/env python

# Copyright STIFTELSEN SINTEF 2016

import suds
import urllib2
import sys
if len(sys.argv) < 3:
    print ("Usage:")
    print ("\t %s gss-url token [minimal]" % sys.argv[0])
    print ("\t (got ", len(sys.argv), " arguments" )
    exit()

# get url: 
url = sys.argv[1]
sessionToken = sys.argv[2]

wsdlLocation = "https://api.caxman.eu/sintef/infrastructure/gss-0.1/FileUtilities?wsdl"

useMinimal = False
if len(sys.argv) > 3:
    if sys.argv[3] == "minimal":
        useMinimal = True

client = suds.client.Client(wsdlLocation)
if useMinimal:
    listFiles = client.service.listFilesMinimal(url, sessionToken)
else :
    listFiles = client.service.listFiles(url, sessionToken)
print listFiles
