/*
 * Copyright STIFTELSEN SINTEF 2016
 */

package no.sintef.gss.frontend;

import com.sun.istack.logging.Logger;
import java.io.BufferedReader;
import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import javax.net.ssl.HttpsURLConnection;
import no.sintef.gss.ws.IOException_Exception;
import no.sintef.gss.ws.ResourceInformation;


/**
 * Used for accessing the GSS in the CloudFlow cloud
 * @author kjetilo
 */
public class GenericFileStorage {
    static {
       	System.setProperty("jsse.enableSNIExtension", "false");
    }
    final private String sessionToken;

    
    final private no.sintef.gss.ws.FileUtilities fileUtilities;
    
    /**
     * @param url the URL to the WSDL document to use, ie. the server URL,
     *            i.e. https://api.eu-cloudflow.eu/sintef/gss-0.1/FileUtilities?wsdl
     * @param sessionToken the relevant sessionToken 
     */
    public GenericFileStorage(String sessionToken, URL url) {
        this.sessionToken = sessionToken;
        fileUtilities = new no.sintef.gss.ws.FileUtilities_Service(url).getFileUtilitiesPort();
    }
    
    /**
     * Creates a new instance using the default URL.
     * @param sessionToken the relevant sessionToken (returned from Keystone)
     * @note This uses the default url for the server.
     */
    public GenericFileStorage(String sessionToken) {
        this.sessionToken = sessionToken;
        fileUtilities = new no.sintef.gss.ws.FileUtilities_Service().getFileUtilitiesPort();
    }
    
    /**
     * Creates a new resource.
     * @param path Relevant path for the new object.
     *             For SWIFT this is "swift://tenant/container/filename",
     *             for PLM this is "plm://REPOSITORY/MODEL/nodeIDParent/filename"
     * @param data the data to write
     * @return a unique FileIdentifier that must be used to reference the file later
     * @throws java.io.IOException If something bad happens with the connection
     */
    public FileIdentifier create(String path, String data) throws IOException {
        return create(path, data, "text/plain");
    }
    
    /**
     * Creates a new resource.
     * @param path Relevant path for the new object.
     *             For SWIFT this is "swift://tenant/container/filename",
     *             for PLM this is "plm://REPOSITORY/MODEL/nodeIDParent/filename"
     * @param data the data to write
     * @param contentType the mimeType of the data, i.e. "text/plain", "text/xml", etc.
     * @return a unique FileIdentifier that must be used to reference the file later
     * @throws java.io.IOException
     */
    public FileIdentifier create(String path, String data, String contentType) throws IOException {
        return create(path, new ByteArrayInputStream(data.getBytes()), 
                data.getBytes().length, contentType);
    }
    
    /**
     * Creates a new resource.
     * @param path Relevant path for the new object.
     *             For SWIFT this is "swift://tenant/container/filename",
     *             for PLM this is "plm://REPOSITORY/MODEL/nodeIDParent/filename"
     * @param data the data to write
     * @param contentLength the length of the content (data) in bytes
     * @return a unique FileIdentifier that must be used to reference the file later
     * @note Assumes a content-type of "text/plain"
     * @throws java.io.IOException
     */
    public FileIdentifier create(String path, InputStream data, int contentLength) throws IOException {
        return create(path, data, contentLength, "application/octet-stream");
    }
    
    /**
     * Creates a new resource.
     * @param path Relevant path for the new object.
     *             For SWIFT this is "swift://tenant/container/filename",
     *             for PLM this is "plm://REPOSITORY/MODEL/nodeIDParent/filename"
     * @param data the data to write
     * @param contentType the mimeType of the data, i.e. "text/plain", "text/xml", etc.
     *                    for binary files it's probably wise to use something like "application/octet-stream"
     * @param contentLength the length of the content (data) in bytes
     * @return a unique FileIdentifier that must be used to reference the file later
     * @throws java.io.IOException
     */
    public FileIdentifier create(String path, InputStream data, int contentLength, String contentType) throws IOException {
        if(data == null || path == null) {
            throw new NullPointerException();
        }
        
        final String resourceURI = path;
        
        try {
            no.sintef.gss.ws.ResourceInformation information = 
                    fileUtilities.getResourceInformation(resourceURI, sessionToken);

            MinimalHttpResponse minimalHttpResponse = 
                        new MinimalHttpResponse(information.getCreateDescription(), data, contentLength, contentType, sessionToken);

            try (BufferedReader reader = new BufferedReader(new InputStreamReader(minimalHttpResponse.read())))
            {
                /*String line;
                String generatedFileURI = "";
                while((line = reader.readLine()) != null) {
                    generatedFileURI += line;
                }*/

                if (information.isQueryForName()) {
                    String filename = minimalHttpResponse.getHeaderField("filename");
                    return new FileIdentifier(filename);
                }

                return new FileIdentifier(path);
            } finally {
                minimalHttpResponse.close();
            }
        }
        catch (IOException_Exception ioe_e) {
            throw new IOException(ioe_e);
        }
    }
    
    /**
     * Reads the resource associated to the id.
     * 
     * @param id The id to read from the storage.
     * @return the content of the given file
     * @throws IOException If reading the inputstream from the storage fails. 
     *                     Typically this happens if there has been a network error.
     */
    public String readString(FileIdentifier id) throws IOException {
        try(BufferedReader reader = new BufferedReader(new InputStreamReader(read(id)))) {
           String wholeString = "";
           String line;
           while((line = reader.readLine()) != null) {
               wholeString += line + "\n";
           }
           return wholeString;
        }
    }
    
    
    /**
     * Reads the resource associated to the id.
     * 
     * @param id The id to read from the storage.
     * @return the content of the given file
     * @throws java.io.IOException
     */
    public InputStream read(FileIdentifier id) throws IOException {
        try {
            ResourceInformation information = 
                    fileUtilities.getResourceInformation(id.getUuid(), sessionToken);
        
            return issueHttpConnection(information.getReadDescription());
        }
        catch (IOException_Exception ioe_e) {
            throw new IOException(ioe_e);
        }
    }
    
    /**
     * Updates the resource with the new content.
     * @param id The id for the resource to update
     * @param data The new data for the resource.
     * @return the unique identifier of the updated resource
     * @throws java.io.IOException and java.net.MalformedUrlException
     */
    public FileIdentifier update(FileIdentifier id, InputStream data, int contentLength, String contentType) throws MalformedURLException, IOException {
        try {
            ResourceInformation information = 
                    fileUtilities.getResourceInformation(id.getUuid(), sessionToken);

            MinimalHttpResponse minimalHttpResponse = 
                        new MinimalHttpResponse(information.getUpdateDescription(), data, contentLength, contentType, sessionToken);

            try (BufferedReader reader = new BufferedReader(new InputStreamReader(minimalHttpResponse.read())))
            {

                if (information.isQueryForName()) {
                    String filename = minimalHttpResponse.getHeaderField("filename");
                    return new FileIdentifier(filename);
                }

                return new FileIdentifier(id.getUuid());
            } finally {
                minimalHttpResponse.close();
            }
        }
        catch (IOException_Exception ioe_e) {
            throw new IOException(ioe_e);
        }
    }
    
    /**
     * Updates the resource with the new content.
     * @param id The id for the resource to update
     * @param data The new data for the resource.
     * @return the unique identifier of the updated resource
     * @throws java.io.IOException
     */
    public FileIdentifier update(FileIdentifier id, String data) throws IOException {
        return update(id, new ByteArrayInputStream(data.getBytes()), data.getBytes().length, "text/plain");
    }
    
    /**
     * Updates the resource with the new content.
     * @param id The id for the resource to update
     * @param data The new data for the resource.
     * @param contentType The type for the data, eg. "text/plain", "text/xml" etc.
     * @return the unique identifier of the updated resource
     */
    public FileIdentifier update(FileIdentifier id, String data, String contentType) throws IOException {
        return update(id, new ByteArrayInputStream(data.getBytes()), data.getBytes().length, contentType);
    }
    
    
    /**
     * Deletes the file from the storage.
     * @param id 
     * @throws java.net.MalformedURLException 
     */
    public void delete(FileIdentifier id) throws MalformedURLException, IOException {
        try {
            ResourceInformation information = 
                    fileUtilities.getResourceInformation(id.getUuid(), sessionToken);

            issueHttpConnection(information.getDeleteDescription()).close();
        }
        catch (IOException_Exception ioe_e) {
            throw new IOException(ioe_e);
        }
    }
    
    /**
     * Checks if the file exists
     * @param id FileIdentifier representing the file we want to check if exists
     * @return true if file exists
     * @throws java.io.IOException
     */
    public boolean contains(FileIdentifier id) throws IOException {
        try {
            return fileUtilities.containsFile(id.getUuid(), sessionToken);
        }
        catch (IOException_Exception ioe_e) {
            throw new IOException(ioe_e);
        }
    }
    
    /**
     * Lists all files within the given path
     * @param path the path to list. 
     *             For SWIFT this is "swift://tenant/container/filename",
     *             for PLM this is "plm://REPOSITORY/MODEL/nodeIDParent/filename"
     * @return a List of files available
     * @throws java.io.IOException
     */
    public List<FileDescription> list(String path) throws IOException {

        try {
            List<no.sintef.gss.ws.ResourceInformation> listFromServer = 
                    fileUtilities.listFiles(path, sessionToken);

            List<FileDescription> files = new ArrayList<>();

            for(no.sintef.gss.ws.ResourceInformation info : listFromServer) {

                files.add(new FileDescription(info.getVisualName(), info.getUniqueName(), 
                            new FileIdentifier(info.getUniqueName()),
                            info.getType()));
            }

            return files;
        }
        catch (IOException_Exception ioe_e) {
            throw new IOException(ioe_e);
        }
    }
    
    /**
     * Describes a file in the storage
     * 
     * @param path The id for the resource to describe.
     * @return A FileDescription element describing the resource
     * @throws IOException
     */
    public FileDescription describeFile(String path) throws IOException {
        try {
            no.sintef.gss.ws.ResourceInformation info = fileUtilities.getResourceInformation(path, sessionToken);
            return new FileDescription(info.getVisualName(), info.getUniqueName(),
                    new FileIdentifier(info.getUniqueName()), info.getType());
        }
        catch (IOException_Exception ioe_e) {
            throw new IOException(ioe_e);
        }
    }
    
    
    /**
     * Creates a folder in the storage
     * 
     * @param path The path and name (path/name) for the new folder
     * @return A FileDescription element describing the newly created folder
     * @throws IOException
     */
    public FileDescription createFolder(String path) throws IOException {
        try {
            no.sintef.gss.ws.ResourceInformation info = fileUtilities.createFolder(path, sessionToken);
            return new FileDescription(info.getVisualName(), info.getUniqueName(),
                new FileIdentifier(info.getUniqueName()), info.getType());
        }
        catch (IOException_Exception ioe_e) {
            throw new IOException(ioe_e);
        }
    }
    
    /**
     * Deletes a folder in the storage
     * 
     * @param path The path to the folder that should be deleted.
     * @return Boolean value based on success of the delete operation
     * @throws IOException
     */
    public boolean deleteFolder(String path) throws IOException {
        try {
            return fileUtilities.deleteFolder(path, sessionToken);
        } catch (IOException_Exception ioe_e) {
            throw new IOException(ioe_e);
        }
    }
    
    
    private InputStream issueHttpConnection(no.sintef.gss.ws.RequestDescription request) throws MalformedURLException, IOException {
        return issueHttpConnection(request, null, 0, "text/plain");
    }
    
    private InputStream issueHttpConnection(no.sintef.gss.ws.RequestDescription request,
            InputStream input, int contentLength, String contentType) throws MalformedURLException, IOException {
        
        if (!request.isSupported()) {
            throw new MalformedURLException("Request is not supported by GSS");
        }
        
        URL url = new URL(request.getUrl());
        
        //System.out.println("GenericFileStorage url: " + request.getUrl() );
        final HttpsURLConnection connection = (HttpsURLConnection)url.openConnection();
        
        
        connection.setRequestMethod(request.getHttpMethod());
        
        // We need to authenticate ourselves
        connection.setRequestProperty(request.getSessionTokenField(), sessionToken);
        
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
                connection.disconnect();
            }
            
            public String getHeaderField(String headerName) {
                return connection.getHeaderField(headerName);
            }
        };
    }

}
