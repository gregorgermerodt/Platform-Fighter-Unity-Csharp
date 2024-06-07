using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;

public class MCHEditorWindow : EditorWindow
{
    [System.Serializable]
    public class CharacterStats
    {
        public string name;
        public float scale;
        public float xpos;
        public float ypos;
        public float zpos;
    }
    private static List<CharacterStats> characterStats;
    private TextAsset csvFile;

    private void OnEnable()
    {
        csvFile = Resources.Load<TextAsset>("CSV-Files/stats");
        characterStats = new List<CharacterStats>();
        ReadCSVFile();
    }

    private void ReadCSVFile()
    {
        if (csvFile == null)
        {
            Debug.LogError("CSV file not loaded");
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(';');

            CharacterStats currCharVal = new CharacterStats();
            currCharVal.name = values[0];
            currCharVal.scale = float.Parse(values[1]);
            currCharVal.xpos = float.Parse(values[2]);
            currCharVal.ypos = float.Parse(values[3]);
            currCharVal.zpos = float.Parse(values[4]);

            characterStats.Add(currCharVal);
        }
    }

    public static CharacterStats GetPlayerValues(string characterName)
    {
        foreach (CharacterStats characterStat in characterStats)
        {
            if (characterStat.name == characterName)
            {
                return characterStat;
            }
        }
        Debug.LogError("No player with the specified name found");
        return new CharacterStats();
    }

    private void SaveToCSV()
    {
        List<string> lines = new List<string>
        {
            "Name;Scale;xPos;yPos;zPos"
        };

        foreach (var character in characterStats)
        {
            string line = $"{character.name};{character.scale};{character.xpos};{character.ypos};{character.zpos}";
            lines.Add(line);
        }

        File.WriteAllLines(GetCSVFilePath(), lines);
        AssetDatabase.Refresh();
        Debug.Log("CSV file saved");
    }

    private string GetCSVFilePath()
    {
        string relativecsvFilePath = "Resources/CSV-Files/stats.csv";
        return Path.Combine(Application.dataPath, relativecsvFilePath);
    }

    [MenuItem(itemName: "Tools/Stats table")]
    public static MCHEditorWindow Open()
    {
        MCHEditorWindow mchEditorWindow = EditorWindow.GetWindow<MCHEditorWindow>(
            title: "Character stats",
            focus: true // set window in foreground => ready for input
        );
        mchEditorWindow.minSize = new Vector2(x: 450.0f, y: 100.0f);
        mchEditorWindow.Show();
        return mchEditorWindow;
    }

    private MultiColumnHeaderState _multiColumnHeaderState;
    private MultiColumnHeader _multiColumnHeader;
    private MultiColumnHeaderState.Column[] _columns;
    private float _multiColumnHeaderWidth;
    private bool _firstOnGUIIterationAfterInitialize;

    private void Initialize()
    {
        _firstOnGUIIterationAfterInitialize = true;
        _multiColumnHeaderWidth = position.width;
        float posColumnsWidth = 50.0f;
        _columns = new MultiColumnHeaderState.Column[]
        {
            new MultiColumnHeaderState.Column()
            {
                allowToggleVisibility = false,
				autoResize = true,
                minWidth = 100.0f,
                maxWidth = 100.0f,
                canSort = true,
                sortingArrowAlignment = TextAlignment.Right,
                headerContent = new GUIContent("Name"),
                headerTextAlignment = TextAlignment.Center,

            },
            new MultiColumnHeaderState.Column()
            {
                allowToggleVisibility = true,
                autoResize = true,
                minWidth = posColumnsWidth,
                maxWidth = posColumnsWidth,
                canSort = false,
                sortingArrowAlignment = TextAlignment.Right,
                headerContent = new GUIContent("Scale"),
                headerTextAlignment = TextAlignment.Center,
            },
            new MultiColumnHeaderState.Column()
            {
                allowToggleVisibility = true,
                autoResize = true,
                minWidth = posColumnsWidth,
                maxWidth = posColumnsWidth,
                canSort = false,
                sortingArrowAlignment = TextAlignment.Right,
                headerContent = new GUIContent("X-Pos"),
                headerTextAlignment = TextAlignment.Center,
            },
            new MultiColumnHeaderState.Column()
            {
                allowToggleVisibility = true,
                autoResize = true,
                minWidth = posColumnsWidth,
                maxWidth = posColumnsWidth,
                canSort = false,
                sortingArrowAlignment = TextAlignment.Right,
                headerContent = new GUIContent("Y-Pos"),
                headerTextAlignment = TextAlignment.Center,
            },
            new MultiColumnHeaderState.Column()
            {
                allowToggleVisibility = true,
                autoResize = true,
                minWidth = posColumnsWidth,
                maxWidth = posColumnsWidth,
                canSort = false,
                sortingArrowAlignment = TextAlignment.Right,
                headerContent = new GUIContent("Z-Pos"),
                headerTextAlignment = TextAlignment.Center,
            },
        };

        _multiColumnHeaderState = new MultiColumnHeaderState(columns: _columns);
        _multiColumnHeader = new MultiColumnHeader(state: _multiColumnHeaderState);

        /*
         * - adds an event handler to the visibleColumnsChanged event list of the MultiColumnHeader
         *   _multiColumnHeader which resizes the columns if the visibility has changed
         * - lambda function is called with a MultiColumnHeader object as an argument each time the
         *   visibleColumnsChanged event is called
         */
        _multiColumnHeader.visibleColumnsChanged += (multiColumnHeader) =>
        {
            multiColumnHeader.ResizeToFit();
        };

        // Initial resizing of the content.
        _multiColumnHeader.ResizeToFit();
    }

    private readonly Color _lighterColor = new Color(r: 1.0f, g: 1.0f, b: 1.0f, a: 0.15f);
    private readonly Color _darkerColor = new Color(r: 0.0f, g: 0.0f, b: 0.0f, a: 0.15f);

    private Vector2 _scrollPosition;

    private void OnGUI()
    {
        // standard unity line height
        float columnHeight = EditorGUIUtility.singleLineHeight;

        // empty space to be able to use GUILayoutUtility.GetLastRect();
        GUILayout.FlexibleSpace();
        // get the rectangle of the empty space (whole window as it is empty)
        Rect windowRect = GUILayoutUtility.GetLastRect();
        // match windowRect to window width and height
        windowRect.width = position.width;
        windowRect.height = position.height;

        if (_multiColumnHeader == null)
        {
            Initialize();
        }

        // makes table background more greyish
        GUIStyle groupGUIStyle = new GUIStyle(GUI.skin.box);

        // pseudo padding (actually the size of the rectangle)
        Vector2 groupRectPaddingInWindow = new Vector2(10.0f, 10.0f);
        Rect groupRect = new Rect(source: windowRect);
        groupRect.x += groupRectPaddingInWindow.x;
        groupRect.y += groupRectPaddingInWindow.y;
        groupRect.width -= groupRectPaddingInWindow.x * 2;
        groupRect.height -= groupRectPaddingInWindow.y * 2;

        // new group => positioning relative to groupRect
        GUI.BeginGroup(position: groupRect, style: groupGUIStyle);
        {   // Groupe scope

            // reset pseudo padding to correctly calculate the scrollView
            groupRect.x -= groupRectPaddingInWindow.x;
            groupRect.y -= groupRectPaddingInWindow.y;

            Rect positionalRectAreaOfScrollView = new Rect(source: groupRect);

            // create a `viewRect` since it should be separate from `rect` to avoid circular dependency.
            Rect viewRect = new Rect(source: groupRect)
            {
                // ensures that the visible area of the ScrollView covers the entire width of the visible columns
                width = _multiColumnHeaderState.widthOfAllVisibleColumns
            };

            // reset pseudo padding to correctly calculate the scrollView (after it was initialized)
            groupRect.width += groupRectPaddingInWindow.x * 2;
            groupRect.height += groupRectPaddingInWindow.y * 2;

            _scrollPosition = GUI.BeginScrollView(
                position: positionalRectAreaOfScrollView,
                scrollPosition: _scrollPosition,
                viewRect: viewRect,
                alwaysShowHorizontal: false,
                alwaysShowVertical: false
            );
            {   // ScrollView Scope.

                // ensures the right multiColumnHeaderWidth
                _multiColumnHeaderWidth = Mathf.Max(positionalRectAreaOfScrollView.width + _scrollPosition.x, _multiColumnHeaderWidth);

                // rectangle for the multi column table.
                Rect columnRectPrototype = new Rect(source: positionalRectAreaOfScrollView)
                {
                    width = _multiColumnHeaderWidth,
                    height = columnHeight,
                };

                // draw header of the columns
                _multiColumnHeader.OnGUI(rect: columnRectPrototype, xScroll: 0.0f);

                float heightJump = columnHeight;

                float rowHeight = 20.0f;

                // fill columns in each row
                for (int a = 0; a < characterStats.Count; a++)
                {
                    CharacterStats currentCharacter = characterStats[a];

                    Rect rowRect = new Rect(source: columnRectPrototype);

                    //rowRect.y += columnHeight * (a + 1); // every second row empty

                    // name field
                    int columnIndex = 0;
                    if (_multiColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                    {
                        // index of the visible columns
                        int visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);
                        // determination of the rectangle of a visible column
                        Rect columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                        // row height
                        columnRect.y = rowRect.y + heightJump;
                        //columnRect.height += heightJump;

                        // style of the field
                        GUIStyle nameFieldGUIStyle = new GUIStyle(GUI.skin.label)
                        {
                            padding = new RectOffset(left: 10, right: 10, top: 2, bottom: 2)
                        };
                        EditorGUI.LabelField(
                            position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                            label: new GUIContent(characterStats[a].name),
                            style: nameFieldGUIStyle
                        );
                    }
                    // scale field
                    columnIndex = 1;
                    if (_multiColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                    {
                        int visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);
                        Rect columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                        columnRect.y = rowRect.y + heightJump;
                        //columnRect.height += heightJump;
                        GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField)
                        {
                            alignment = TextAnchor.MiddleCenter,
                        };
                        currentCharacter.scale = EditorGUI.FloatField(
                            position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                            value: currentCharacter.scale,
                            style: fieldStyle
                        );
                    }
                    // xpos field
                    columnIndex = 2;
                    if (_multiColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                    {
                        int visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);
                        Rect columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                        columnRect.y = rowRect.y + heightJump;
                        //columnRect.height += heightJump;
                        GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField)
                        {
                            alignment = TextAnchor.MiddleCenter,
                        };
                        currentCharacter.xpos = EditorGUI.FloatField(
                            position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                            value: currentCharacter.xpos,
                            style: fieldStyle
                        );
                    }
                    // ypos field
                    columnIndex = 3;
                    if (_multiColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                    {
                        int visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);
                        Rect columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                        columnRect.y = rowRect.y + heightJump;
                        //columnRect.height += heightJump;
                        GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField)
                        {
                            alignment = TextAnchor.MiddleCenter,
                        };
                        currentCharacter.ypos = EditorGUI.FloatField(
                            position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                            value: currentCharacter.ypos,
                            style: fieldStyle
                        );
                    }
                    // zpos field
                    columnIndex = 4;
                    if (_multiColumnHeader.IsColumnVisible(columnIndex: columnIndex))
                    {
                        int visibleColumnIndex = _multiColumnHeader.GetVisibleColumnIndex(columnIndex: columnIndex);
                        Rect columnRect = _multiColumnHeader.GetColumnRect(visibleColumnIndex: visibleColumnIndex);
                        columnRect.y = rowRect.y + heightJump;
                        //columnRect.height += heightJump;
                        GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField)
                        {
                            alignment = TextAnchor.MiddleCenter,
                        };
                        currentCharacter.zpos = EditorGUI.FloatField(
                            position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                            value: currentCharacter.zpos,
                            style: fieldStyle
                        );
                    }

                    // use saved height for each row to draw rectangles to prevent overlapping drawing
                    Rect backgroundColorRect = new Rect(source: rowRect)
                    {
                        y = rowRect.y + heightJump,
                        height = rowHeight
                    };

                    // draw a texture before drawing each of the fields for the whole row.
                    if (a % 2 == 0)
                        EditorGUI.DrawRect(rect: backgroundColorRect, color: _darkerColor);
                    else
                        EditorGUI.DrawRect(rect: backgroundColorRect, color: _lighterColor);

                    heightJump += rowHeight;
                }
            }
            GUI.EndScrollView(handleScrollWheel: true);

        }
        GUI.EndGroup();

        if (GUILayout.Button("Save to csv-file"))
        {
            SaveToCSV();
        }

        if (!_firstOnGUIIterationAfterInitialize)
        {
            //! Uncomment this if you want to have appropriate ResizeToFit(). It brings jaggedness which I didn't like, so I have removed it knowing the implications.

            //float difference = 50.0f;

            ////! Magic number `difference` is just a number to ensure that this width won't be exceeded by this auto scale bug. Lowering this number could cause it to reappear.
            ////? If you don't mind resizing to fit to sometimes overscale columns then just remove these next 2 lines of code.
            //if (_multiColumnHeaderWidth - _multiColumnHeaderState.widthOfAllVisibleColumns > difference)
            //	_multiColumnHeaderWidth -= difference;
        }

        _firstOnGUIIterationAfterInitialize = false;
    }

    private void Awake()
    {
        Initialize();
    }
}
#endif