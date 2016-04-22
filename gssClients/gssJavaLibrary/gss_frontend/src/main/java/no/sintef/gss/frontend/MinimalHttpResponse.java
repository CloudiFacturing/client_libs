/*
 * Copyright STIFTELSEN SINTEF 2016
 */

package no.sintef.gss.frontend;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.MalformedURLException;
import java.net.URL;
import javax.net.ssl.HttpsURLConnection;

/**
 *
 * @author havahol
 */
public class MinimalHttpResponse {
 
    
    
    private HttpsURLConnection connection;
    private URL url;
    private String sessionToken;
    
    public MinimalHttpResponse(no.sintef.gss.ws.RequestDescription request, String sessionToken) throws MalformedURLException, IOException {
        this(request, null, 0, "text/plain", sessionToken);
    }
    
    public MinimalHttpResponse(no.sintef.gss.ws.RequestDescription request,
            InputStream input, int contentLength, String contentType, String sessionToken) throws MalformedURLException, IOException {
        
        connection = null;
        
        if (!request.isSupported()) {
            throw new MalformedURLException("Request is not supported by GSS");
        }
        
        this.sessionToken = sessionToken;
        url = new URL(request.getUrl());
        
        // Open https connection
        connection = (HttpsURLConnection)url.openConnection();
        connection.setRequestMethod(request.getHttpMethod());
        
        // Set authentication
        connection.setRequestProperty(request.getSessionTokenField(), this.sessionToken);
        for(no.sintef.gss.ws.HttpHeaderField header : request.getHeaders()) {
            connection.setRequestProperty(header.getKey(), header.getValue());
        }
        
        connection.setDoInput(true);
        connection.setDoOutput(true);
        
        if (input != null) {
            connection.setRequestProperty("Content-Length", contentLength + "");
            connection.setRequestProperty("Content-Type", contentType + "");
            try (OutputStream outputStream = connection.getOutputStream()) {
                // Write output
                int bytesRead;
                byte[] buffer = new byte[1024];
                while ((bytesRead = input.read(buffer)) > 0) {
                    outputStream.write(buffer, 0, bytesRead);
                }
            }
        }
    }
    
    public InputStream read() throws IOException {
        final InputStream connectionInput = connection.getInputStream();
        return new InputStream() {

            @Override
            public int read() throws IOException {
                return connectionInput.read();
            }

            @Override
            public void close() throws IOException {
                super.close();
                connectionInput.close();
                //connection.disconnect();
            }
        };        
    }
    
    public String getHeaderField(String headerField) {
        
        return connection.getHeaderField(headerField);
    }
    
    public void close() {
        if (connection != null) {
            connection.disconnect();
        }
    }
}
    
 