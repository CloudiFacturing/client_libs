/*
 * Copyright STIFTELSEN SINTEF 2016
 */

package no.sintef.gss.frontend;

/**
 * @author kjetilo
 */
public class FileIdentifier {
    private String uuid;
    
    public FileIdentifier(String uuid) {
        this.uuid = uuid;
    }

    /**
     * @return the uuid
     */
    public String getUuid() {
        return uuid;
    }

    /**
     * @param uuid the uuid to set
     */
    public void setUuid(String uuid) {
        this.uuid = uuid;
    }
    
    /**
     * Convenience method for creating a new PLM ID to reference the PLM database.
     * @note this does not create any data!
     * @param repository The repository to use
     * @param model The model to use
     * @param nodeId The nodeID
     * @return a FileIdentifier referencing the given node
     */
    public static FileIdentifier plmIdentifier(String repository, String model, String nodeId) {
        return new FileIdentifier("plm://" + repository + "/" + model + "/" + nodeId);
    }
    
    
    /**
     * Convenience method for creating filePaths
     * @param filePath
     * @return a FileIdentifier referencing the given file
     */
    public static FileIdentifier swiftIdentifier(String tenant, String container, String filename) {
        return new FileIdentifier("swift://" + tenant + "/" + container + "/" + filename);
    }
    
}
