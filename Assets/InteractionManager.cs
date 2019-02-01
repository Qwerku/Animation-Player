using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.IO;
using System;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour {

    //todo:  commit this project to github 
    //todo: create play animation button

    public Camera camera;
    public InputField animationFolderInput;
    public Text folderNotExistTextElement;
    public Button clearNoticeButton;

    private Vector3[,] positionArray;
    private Quaternion[,] rotationArray;
    private Vector3[,] localScaleArray;
    private int[] delayToNextFrameArray;
    
    private List<string> animationFilePaths = new List<string>();

    private Transform objectTransform;
    private Renderer objectRender;
    private GameObject objectSelected;
    private Color objectOldColor;
    private Color selectedColor = Color.green;

    private List<GameObject> shapeList = new List<GameObject>();
    private List<string> shapeTypeList = new List<string>();
    
    private int numAnimationFiles = 0;
    private int currentFrameNumber = 0;
    private int numberOfShapes = 0;
    
    private DirectoryInfo workingDirectory = null;
    
    void Start () {
        ClearAndInvisErrorText();
	}
	
	void Update () {

    }

    public void UpdateAnimationFolderAndCreateObjects()
    {
        workingDirectory = new DirectoryInfo(Application.dataPath + "/Animation/" + animationFolderInput.text + "/");
        if (workingDirectory.Exists)
        {
            FileInfo[] fileInfoList = workingDirectory.GetFiles("*.txt");
            animationFilePaths.Clear();
            DestroyExistingShapes();
            numAnimationFiles = fileInfoList.Length;
            delayToNextFrameArray = null;
            currentFrameNumber = 0;

            for (int i = 0; i < numAnimationFiles; i++)
            {
                animationFilePaths.Add(fileInfoList[i].FullName);
            }

            numberOfShapes = FindNumberOfShapesFromFile();
            if (numberOfShapes > 0)
            {
                positionArray = new Vector3[numAnimationFiles, numberOfShapes];
                rotationArray = new Quaternion[numAnimationFiles, numberOfShapes];
                localScaleArray = new Vector3[numAnimationFiles, numberOfShapes];
                delayToNextFrameArray = new int[numAnimationFiles];

                for (int i = 0; i < numAnimationFiles; i++)
                {
                    LoadFrameFromFile(i);
                }
            }

        }
        else
        {
            numAnimationFiles = 0;
            numberOfShapes = 0;
            SetErrorText("FOLDER DOESNT EXIST!");
            //clearNoticeButton.interactable = false;   //this is how you grey out a button
        }

        if ((numberOfShapes >0) && (numAnimationFiles > 0))
        {
            CreateAllObjectsToAnimate();
        }
    }

    void CreateAllObjectsToAnimate()
    {
        for (int i = 0; i<numberOfShapes; i++)
        {
            if (shapeTypeList[i] == "cube")  //shapeTypeList is created/defined in LoadFrameFromFile(frame = 0)
            {
                shapeList.Add(GameObject.CreatePrimitive(PrimitiveType.Cube));
                shapeList[i].name = i.ToString();
                shapeList[i].transform.position = positionArray[0, i]; //0 is frame 0, i is shapeNumber
                shapeList[i].transform.rotation = rotationArray[0, i];
                shapeList[i].transform.localScale = localScaleArray[0, i];
            }
        }
    }

    void DestroyExistingShapes()
    {
        for (int i = 0; i < numberOfShapes; i++)
        {
            GameObject.Destroy(shapeList[i]);
        }
        shapeList.Clear();
        shapeTypeList.Clear();
        positionArray = null;
        rotationArray = null;
        localScaleArray = null;
        numberOfShapes = 0;
    }

    void LoadFrameFromFile(int FrameNumberToLoad)  //puts shape info from file into positionArray, rotationArray, and localScaleArray
    {
        bool loadFrameError = false;
        string allFileContents;
        string[] arrayOfObjects, parametersForEachObject;
        int delayToNextFrame = 0;
   
        allFileContents = "";
        try
        {
            allFileContents = File.ReadAllText(animationFilePaths[FrameNumberToLoad]);
        }
        catch (Exception e)
        {
            loadFrameError = true;
            SetErrorText("FILE ERROR LOADING FRAME #" + FrameNumberToLoad.ToString());
            Debug.LogException(e, this);
        }

        if ((allFileContents != "") && (!loadFrameError))
        {
            arrayOfObjects = allFileContents.Split("***************".ToCharArray());
            delayToNextFrame = PullIntValueWithStringKeyFromStringBody(":msdelaytonextframe", arrayOfObjects);
            delayToNextFrameArray[FrameNumberToLoad] = delayToNextFrame;

            for (int objectNum = 1; objectNum < arrayOfObjects.Length; objectNum++)  //start at 1 b/c 0 is the framedelay string
            {
                if ((arrayOfObjects[objectNum].Length > 1) && (!loadFrameError))  //if not an empty line from the previous parse
                {
                    parametersForEachObject = arrayOfObjects[objectNum].Split(System.Environment.NewLine.ToCharArray());
                    string shapeType = "";
                    int shapeNumber = 0;
                    float positionX = 0.0f;
                    float positionY = 0.0f;
                    float positionZ = 0.0f;
                    float rotationX = 0.0f;
                    float rotationY = 0.0f;
                    float rotationZ = 0.0f;
                    float rotationW = 0.0f;
                    float localScaleX = 0.0f;
                    float localScaleY = 0.0f;
                    float localScaleZ = 0.0f;

                    try
                    {
                        shapeType = PullStringValueWithStringKeyFromStringBody(":shapetype", parametersForEachObject);
                        shapeNumber = PullIntValueWithStringKeyFromStringBody(":shapenumber", parametersForEachObject);
                        positionX = PullFloatValueWithStringKeyFromStringBody(":positionx", parametersForEachObject);
                        positionY = PullFloatValueWithStringKeyFromStringBody(":positiony", parametersForEachObject);
                        positionZ = PullFloatValueWithStringKeyFromStringBody(":positionz", parametersForEachObject);
                        rotationX = PullFloatValueWithStringKeyFromStringBody(":rotationx", parametersForEachObject);
                        rotationY = PullFloatValueWithStringKeyFromStringBody(":rotationy", parametersForEachObject);
                        rotationZ = PullFloatValueWithStringKeyFromStringBody(":rotationz", parametersForEachObject);
                        rotationW = PullFloatValueWithStringKeyFromStringBody(":rotationw", parametersForEachObject);
                        localScaleX = PullFloatValueWithStringKeyFromStringBody(":localScalex", parametersForEachObject);
                        localScaleY = PullFloatValueWithStringKeyFromStringBody(":localScaley", parametersForEachObject);
                        localScaleZ = PullFloatValueWithStringKeyFromStringBody(":localScalez", parametersForEachObject);
                    }
                    catch (Exception e)
                    {
                        loadFrameError = true;
                        SetErrorText("MALFORMED PARAMETERS IN FRAME FILE!");
                        Debug.LogException(e, this);
                    }

                    if (!loadFrameError)
                    {
                        if (shapeType == "cube")
                        {
                            positionArray[FrameNumberToLoad, shapeNumber] = new Vector3(positionX, positionY, positionZ);
                            rotationArray[FrameNumberToLoad, shapeNumber] = new Quaternion(rotationX, rotationY, rotationZ, rotationW);
                            localScaleArray[FrameNumberToLoad, shapeNumber] = new Vector3(localScaleX, localScaleY, localScaleZ);

                            if (FrameNumberToLoad == 0)  //do this only once for the entire folder
                            {
                                shapeTypeList.Add(shapeType);
                            }
                        }
                        else
                        {
                            loadFrameError = true;
                            SetErrorText("Unsupported shape type found in frame! (shape#" + shapeNumber.ToString() + ")");
                        }
                    }
                }
            }
        }        
    }

    int FindNumberOfShapesFromFile()
    {

        bool loadFrameError = false;
        string allFileContents;
        string[] arrayOfObjects, parametersForEachObject;
        int delayToNextFrame = 0;

        allFileContents = "";
        try
        {
            allFileContents = File.ReadAllText(animationFilePaths[0]);
        }
        catch (Exception e)
        {
            loadFrameError = true;
            SetErrorText("FILE ERROR LOADING FRAME #" + 0.ToString());
            Debug.LogException(e, this);
            return -1;
        }

        if ((allFileContents != "") && (!loadFrameError))
        {
            arrayOfObjects = allFileContents.Split("***************".ToCharArray());
            delayToNextFrame = PullIntValueWithStringKeyFromStringBody(":msdelaytonextframe", arrayOfObjects);
            int maxShapeNumberFound = 0;

            for (int objectNum = 1; objectNum < arrayOfObjects.Length; objectNum++)  //start at 1 b/c 0 is the framedelay string
            {
                if ((arrayOfObjects[objectNum].Length > 1) && (!loadFrameError))  //if not an empty line from the previous parse
                {

                    parametersForEachObject = arrayOfObjects[objectNum].Split(System.Environment.NewLine.ToCharArray());
                    int shapeNumber = 0;

                    try
                    {
                        shapeNumber = PullIntValueWithStringKeyFromStringBody(":shapenumber", parametersForEachObject);
                        if (shapeNumber > maxShapeNumberFound)
                        {
                            maxShapeNumberFound = shapeNumber;
                        }
                    }
                    catch (Exception e)
                    {
                        loadFrameError = true;
                        SetErrorText("MALFORMED PARAMETERS IN FRAME FILE!");
                        Debug.LogException(e, this);
                        return -1;
                    }


                }
            }
            return maxShapeNumberFound+1;  //shape numbers start at zero, so add 1 here
        }
        else
        {
            SetErrorText("FILE CONTENTS EMPTY!");
            return -1;
        }
    }

    int PullIntValueWithStringKeyFromStringBody(string stringKey, string[] entireStringBody)
    {
        int parameterIndex = FindIndexInParameterArrayWithStringKey(stringKey, entireStringBody);
        //Find which parameter line contains the stringkey you're looking for

        return int.Parse(entireStringBody[parameterIndex].Substring(0, entireStringBody[parameterIndex].IndexOf(stringKey)));
        //pull and return the number that occurs before the string key on that particular line of the file
    }

    string PullStringValueWithStringKeyFromStringBody(string stringKey, string[] entireStringBody)
    {
        int parameterIndex = FindIndexInParameterArrayWithStringKey(stringKey, entireStringBody);
        //Find which parameter line contains the stringkey you're looking for

        return entireStringBody[parameterIndex].Substring(0, entireStringBody[parameterIndex].IndexOf(stringKey));
        //pull and return the string that occurs before the string key on that particular line of the file
    }

    float PullFloatValueWithStringKeyFromStringBody(string stringKey, string[] entireStringBody)
    {
        int parameterIndex = FindIndexInParameterArrayWithStringKey(stringKey, entireStringBody);
        //Find which parameter line contains the stringkey you're looking for

        return float.Parse(entireStringBody[parameterIndex].Substring(0, entireStringBody[parameterIndex].IndexOf(stringKey)));
        //pull and return the number that occurs before the string key on that particular line of the file
    }

    int FindIndexInParameterArrayWithStringKey(string stringKey, string[] objectParameterArray)
    {
        //each index in objectParameterArray is a single line in the file
        //Find and return which line contains the stringkey you're looking for

        int indexOfKey;
        for (int i = 0; i < objectParameterArray.Length; i++)
        {
            indexOfKey = (objectParameterArray[i].IndexOf(stringKey));
            if (indexOfKey > -1)
            {
                return i;
            }
        }
        return -1;
    }
              
    void SetErrorText(string errorText)
    {
        folderNotExistTextElement.enabled = true;   //make the text visible
        folderNotExistTextElement.text = errorText;
        clearNoticeButton.gameObject.SetActive(true);  //make the button visible
    }

    public void ClearAndInvisErrorText()
    {
        folderNotExistTextElement.enabled = false;   //make text invisible
        clearNoticeButton.gameObject.SetActive(false);   //make the button invisible and noninteractable
    }

    string GetStringOfAllObjectInfo()
    {
        string holderString = "";
        Transform holderTransform;
                
        //holderString = holderString + int.Parse(msDelayToNextFrame.text) + ":msdelaytonextframe" + System.Environment.NewLine;
        for (int i = 0; i < numberOfShapes; i++)
        {
            holderString = holderString + "***************" + System.Environment.NewLine;
            holderString = holderString + shapeTypeList[i] + ":shapetype" + System.Environment.NewLine;
            holderString = holderString + i.ToString() + ":shapenumber" + System.Environment.NewLine;
            holderTransform = shapeList[i].transform;
            holderString = holderString + holderTransform.position.x.ToString() + ":positionx" +  System.Environment.NewLine;
            holderString = holderString + holderTransform.position.y.ToString() + ":positiony" + System.Environment.NewLine;
            holderString = holderString + holderTransform.position.z.ToString() + ":positionz" + System.Environment.NewLine;

            holderString = holderString + holderTransform.rotation.x.ToString() + ":rotationx" +  System.Environment.NewLine;
            holderString = holderString + holderTransform.rotation.y.ToString() + ":rotationy" + System.Environment.NewLine;
            holderString = holderString + holderTransform.rotation.z.ToString() + ":rotationz" + System.Environment.NewLine;
            holderString = holderString + holderTransform.rotation.w.ToString() + ":rotationw" + System.Environment.NewLine;

            holderString = holderString + holderTransform.localScale.x.ToString() + ":localScalex" + System.Environment.NewLine;
            holderString = holderString + holderTransform.localScale.y.ToString() + ":localScaley" + System.Environment.NewLine;
            holderString = holderString + holderTransform.localScale.z.ToString() + ":localScalez" + System.Environment.NewLine;
                
            //holderString = holderString + "***************" + System.Environment.NewLine;
            //holderString = holderString + System.Environment.NewLine;
        }
               
        return holderString;
    }

}

//known todos:

//tenative done pile