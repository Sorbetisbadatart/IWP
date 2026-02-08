using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class StringListSearchProvider : ScriptableObject, ISearchWindowProvider
{
    private string[] listItems;
    private Action<string> onSetIndexCallback;
    public StringListSearchProvider(string[] items, Action<string> callback)
    {
        listItems = items;
        onSetIndexCallback = callback;
    }
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> list = new List<SearchTreeEntry>();
        list.Add(new SearchTreeGroupEntry(new GUIContent("List"), 0));

        //sort list by item class then name
        List<string> sortedListItems = listItems.ToList();
        sortedListItems.Sort((a, b) =>
        {
            string[] splits1 = a.Split('/');
            string[] splits2 = b.Split('/');
            for (int i = 0; i < splits1.Length; i++)
            {
                if (i >= splits2.Length)
                {
                    return 1;
                }
                int value = splits1[i].CompareTo(splits2);
                if (value != 0)
                {
                    if (splits1.Length != splits2.Length && (i == splits1.Length - 1 || i == splits2.Length - 1))
                        return splits1.Length < splits2.Length ? 1 : -1;
                    return value;
                }

            }
            return 0;
        });

        //create a list to track our groups 
        List<string> groups = new List<string>();
        //iterate the list
        foreach (string item in sortedListItems)
        {
            //break items to subcategories to path
            string[] entryTitle = item.Split('/');
            string groupName = "";
            //go throughs path to see if category exists
            for (int i = 0; i < entryTitle.Length - 1; i++)
            {
                groupName += entryTitle[i];
                if (!groups.Contains(groupName))
                {
                    list.Add(new SearchTreeGroupEntry(new GUIContent(entryTitle[i]), i + 1));
                    groups.Add(groupName);
                }
                groupName += "/";
            }
            //create new entry for the last path
            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(entryTitle.Last()));
            entry.level = entryTitle.Length;
            entry.userData = entryTitle.Last();
            list.Add(entry);          
        }
        return list;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        onSetIndexCallback?.Invoke((string)SearchTreeEntry.userData);
        return true;
    }

}

public class SampleSearchProviderUsageEditor : Editor
{

    public override void OnInspectorGUI()
    {
       // Object item = (Object)target;

        //button for dropdown that opens the searchwindow
        //if (GUILayout.Button($"{item.selectedItem}", EditorStyles.popup))
        //{
        //    SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), new StringListSearchProvider());
        //}
    }
}
