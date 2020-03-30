using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueSystemSpriteChanger : MonoBehaviour
{
    private static DialogueSystemSpriteChanger Instance;

    /*NOW THE REAL FUN BEGINS NOW!!!
    DialogueSystemSpriteChanger will be responsible for the changing of the sprite when the commands
    EXPRESSION and POSE is defined in the specified .dsf file.

    So the best thing to do for this one is to have an enumerator so see if it's for
    expressions, or if it's for poses.

    Then again, you can just have a sprite changer for expression, while it still included
    different poses of the character. In this case, I have 2 seperate ones.

    Our mission now is to get EXPRESSION and POSES to work. This is crunch time! We ain't sleeping
    for this one, because I'm simply too excited.
    */

    //So this will read from the <EXPRESSIONS> and <POSES> tags that are found in the .dsf file.

    /*StorySpriteElement take in a identifier, an index, and an image that is associated with it.
     I mean... I could use  a dictionary for that, but since this is a game jam, this is something that
     I'll have to do to assure that I can get this done faster.*/

    [Serializable]
    public class StorySpriteElement
    {
        [SerializeField]
        private string identifier = "";

        [SerializeField]
        private Sprite image = null;

        public string Get_Identifier_Value() => identifier;

        public Sprite Get_Image() => image;
    }

    public enum CHANGEFOR
    {
        EXPRESSION,
        POSE
    }


    [SerializeField]
    private string characterName = null;

    [SerializeField]
    private CHANGEFOR changeFor = default;

    [SerializeField]
    private List<StorySpriteElement> storySpriteElements = new List<StorySpriteElement>();

    //Our sprite render
    private SpriteRenderer spriteRenderer = null;

    //Prefix is a character man plus _EXPRESSION
    private string prefix;

    void Awake()
    {
        Instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();

        //We modify the name as quickly as possible
        prefix = characterName.ToUpper() +  '_' + Enum.GetName(typeof(CHANGEFOR), changeFor);

    }

    public string Get_Prefix() => prefix;

    /// <summary>
    /// Change the image based on its identifier
    /// </summary>
    /// <param name="_identifier"></param>
    public void CHANGE_IMAGE(string _identifier)
    {
        //We iterate through our list first...
        for(int i = 0; i < storySpriteElements.Count; i++)
        {
            //We find the identifer that matches what we're looking for
            if (storySpriteElements[i].Get_Identifier_Value() == _identifier)
            {
                //Now we change the image in the sprite renderer
                spriteRenderer.sprite = storySpriteElements[i].Get_Image();
            }
        }
    }
}
