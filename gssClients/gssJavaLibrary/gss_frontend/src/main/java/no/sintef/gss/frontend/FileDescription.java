/*
 * Copyright STIFTELSEN SINTEF 2016
 */

package no.sintef.gss.frontend;

/**
 *
 * @author kjetilo
 */
public class FileDescription {
    private String visualName;
    private String uniqueName;
    private FileIdentifier id;
    private String type;

    public FileDescription(String visualName, String uniqueName, FileIdentifier id, String type) {
        this.visualName = visualName;
        this.uniqueName = uniqueName;
        this.id = id;
        this.type = type;
    }

    public FileDescription() {}
    
    
    /**
     * @return the visualName
     */
    public String getVisualName() {
        return visualName;
    }

    /**
     * @param visualName the visualName to set
     */
    public void setVisualName(String visualName) {
        this.visualName = visualName;
    }

    /**
     * @return the uniqueName
     */
    public String getUniqueName() {
        return uniqueName;
    }

    /**
     * @param uniqueName the uniqueName to set
     */
    public void setUniqueName(String uniqueName) {
        this.uniqueName = uniqueName;
    }

    /**
     * @return the id
     */
    public FileIdentifier getId() {
        return id;
    }

    /**
     * @param id the id to set
     */
    public void setId(FileIdentifier id) {
        this.id = id;
    }

    /**
     * @return the type
     */
    public String getType() {
        return type;
    }

    /**
     * @param type the type to set
     */
    public void setType(String type) {
        this.type = type;
    }
}
