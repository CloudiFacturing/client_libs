#!/bin/env python

# Copyright STIFTELSEN SINTEF 2016

import suds
import urllib2
import sys
if len(sys.argv) < 2:
    print ("Usage:")
    print ("\t %s token" % sys.argv[0])
    print ("\t (got ", len(sys.argv), " arguments" )
    exit()

# get arguments
token = sys.argv[1]

wsdlLocation = "https://api.caxman.eu/sintef/infrastructure/authManager/AuthManager?wsdl"

client = suds.client.Client(wsdlLocation)
print client.service.validateSessionToken(token)


