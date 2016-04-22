#!/bin/env python

# Copyright STIFTELSEN SINTEF 2016

import suds
import urllib2
import sys
if len(sys.argv) < 4:
    print ("Usage:")
    print ("\t %s username password project" % sys.argv[0])
    print ("\t (got ", len(sys.argv), " arguments" )
    exit()

# get arguments:
username = sys.argv[1]
password = sys.argv[2]
project = sys.argv[3]

wsdlLocation = "https://api.caxman.eu/sintef/infrastructure/authManager/AuthManager?wsdl"

client = suds.client.Client(wsdlLocation)
token = client.service.getSessionToken(username, password, project)
print token

