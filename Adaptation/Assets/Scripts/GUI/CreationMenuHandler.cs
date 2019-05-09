using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreationMenuHandler : MonoBehaviour
{
    [SerializeField]
    Text[] attributeTexts;
    [SerializeField]
    Text[] featureTexts;
    [SerializeField]
    Text[] featureButtonTexts;

    [SerializeField]
    Button completeButton;

    [SerializeField]
    int totalAttributes;
    [SerializeField]
    int totalFeatures;

    int attributes, features;

    int health, attackDamage, attackSpeed, attackRange, movementSpeed;
    bool melee, ranged, block, dash;

    private void Start()
    {
        UpdateGUI();
    }

    public void CompletePlayer()
    {
        GenFilesManager.SavePlayer(GetPlayerTraits());
        SceneManager.LoadScene(6);
    }

    private string GetPlayerTraits()
    {
        StringBuilder playertraits = new StringBuilder();

        for (int i = 0; i < health; i++)
            playertraits.Append('h');

        for (int i = 0; i < attackDamage; i++)
            playertraits.Append('d');

        for (int i = 0; i < attackSpeed; i++)
            playertraits.Append('s');

        for (int i = 0; i < attackRange; i++)
            playertraits.Append('r');

        for (int i = 0; i < movementSpeed; i++)
            playertraits.Append('m');

        playertraits.Append('|');

        if (melee)
            playertraits.Append('M');

        if (ranged)
            playertraits.Append('R');

        if (block)
            playertraits.Append('B');

        if (dash)
            playertraits.Append('D');

        return playertraits.ToString();
    }

    public void UpdateGUI()
    {
        attributeTexts[0].text = "Health: " + health;
        attributeTexts[1].text = "Attack Damage: " + attackDamage;
        attributeTexts[2].text = "Attack Speed: " + attackSpeed;
        attributeTexts[3].text = "Attack Range: " + attackRange;
        attributeTexts[4].text = "Movement Speed: " + movementSpeed;
        attributeTexts[5].text = "Atrribute Points Left: " + (totalAttributes - attributes);

        featureTexts[0].text = "Melee Attack: " + melee;
        featureTexts[1].text = "Ranged Attack: " + ranged;
        featureTexts[2].text = "Block: " + block;
        featureTexts[3].text = "Dash: " + dash;
        featureTexts[4].text = "Feature Points Left: " + (totalFeatures - features);

        featureButtonTexts[0].text = melee ? "x" : "o";
        featureButtonTexts[1].text = ranged ? "x" : "o";
        featureButtonTexts[2].text = block ? "x" : "o";
        featureButtonTexts[3].text = dash ? "x" : "o";

        if (totalAttributes - attributes == 0
            && totalFeatures - features == 0)
        {
            completeButton.interactable = true;
        }
        else
        {
            completeButton.interactable = false;
        }
    }

    public void IncreaseHealth()
    {
        if (attributes < totalAttributes)
        {
            attributes++;
            health++;
        }
        UpdateGUI();
    }

    public void DecreaseHealth()
    {
        if (health > 0)
        {
            attributes--;
            health--;
        }
        UpdateGUI();
    }

    public void IncreaseAttackDamage()
    {
        if (attributes < totalAttributes)
        {
            attributes++;
            attackDamage++;
        }
        UpdateGUI();
    }

    public void DecreaseAttackDamage()
    {
        if (attackDamage > 0)
        {
            attributes--;
            attackDamage--;
        }
        UpdateGUI();
    }

    public void IncreaseAttackSpeed()
    {
        if (attributes < totalAttributes)
        {
            attributes++;
            attackSpeed++;
        }
        UpdateGUI();
    }

    public void DecreaseAttackSpeed()
    {
        if (attackSpeed > 0)
        {
            attributes--;
            attackSpeed--;
        }
        UpdateGUI();
    }

    public void IncreaseAttackRange()
    {
        if (attributes < totalAttributes)
        {
            attributes++;
            attackRange++;
        }
        UpdateGUI();
    }

    public void DecreaseAttackRange()
    {
        if (attackRange > 0)
        {
            attributes--;
            attackRange--;
        }
        UpdateGUI();
    }

    public void IncreaseMovementSpeed()
    {
        if (attributes < totalAttributes)
        {
            attributes++;
            movementSpeed++;
        }
        UpdateGUI();
    }

    public void DecreaseMovementSpeed()
    {
        if (movementSpeed > 0)
        {
            attributes--;
            movementSpeed--;
        }
        UpdateGUI();
    }

    public void ToggleMelee()
    {
        if (features < totalFeatures && !melee)
        {
            features++;
            melee = true;
        }
        else if (melee)
        {
            features--;
            melee = false;
        }
        UpdateGUI();
    }

    public void ToggleRanged()
    {
        if (features < totalFeatures && !ranged)
        {
            features++;
            ranged = true;
        }
        else if (ranged)
        {
            features--;
            ranged = false;
        }
        UpdateGUI();
    }

    public void ToggleBlock()
    {
        if (features < totalFeatures && !block)
        {
            features++;
            block = true;
        }
        else if (block)
        {
            features--;
            block = false;
        }
        UpdateGUI();
    }

    public void ToggleDash()
    {
        if (features < totalFeatures && !dash)
        {
            features++;
            dash = true;
        }
        else if (dash)
        {
            features--;
            dash = false;
        }
        UpdateGUI();
    }
}