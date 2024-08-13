using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;

//public class Test
//{
//    float a = ParameterTable.characterStats[0].AIR_DRIFT_SPEED;
//}

public class ParameterTable : EditorWindow
{
    public FighterStats fighterStats;
    private TextAsset csvFile;

    private void OnEnable()
    {
        csvFile = Resources.Load<TextAsset>("CSV-Files/stats");
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


            fighterStats.NAME = values[0];
            fighterStats.WALKING_SPEED = float.Parse(values[1]);
            fighterStats.HORIZONTAL_GROUND_DECELERATION = float.Parse(values[2]);
            fighterStats.FALL_SPEED = float.Parse(values[3]);
            fighterStats.FALL_GRAVITY_ACCELERATION = float.Parse(values[4]);
            fighterStats.AIR_DRIFT_SPEED = float.Parse(values[5]);
            fighterStats.AIR_DRIFT_ACCELERATION = float.Parse(values[6]);
            fighterStats.SHORTJUMP_JUMP_SPEED = float.Parse(values[7]);
            fighterStats.SHORTJUMP_AIR_DECELERATION = float.Parse(values[8]);
            fighterStats.FULLJUMP_JUMP_SPEED = float.Parse(values[9]);
            fighterStats.FULLJUMP_AIR_DECELERATION = float.Parse(values[10]);
            fighterStats.AIRJUMP_JUMP_SPEED = float.Parse(values[11]);
            fighterStats.AIRJUMP_AIR_DECELERATION = float.Parse(values[12]);
            fighterStats.MAX_AIR_JUMP_COUNT = float.Parse(values[13]);
        }
    }

    private void SaveToCSV()
    {
        List<string> lines = new List<string>
        {
            "NAME;" +
            "WALKING_SPEED;" +
            "HORIZONTAL_GROUND_DECELERATION;" +
            "FALL_SPEED;" +
            "FALL_GRAVITY_ACCELERATION;" +
            "AIR_DRIFT_SPEED;" +
            "AIR_DRIFT_ACCELERATION;" +
            "SHORTJUMP_JUMP_SPEED;" +
            "SHORTJUMP_AIR_DECELERATION;" +
            "FULLJUMP_JUMP_SPEED;" +
            "FULLJUMP_AIR_DECELERATION;" +
            "AIRJUMP_JUMP_SPEED;" +
            "AIRJUMP_AIR_DECELERATION;" +
            "MAX_AIR_JUMP_COUNT;"
        };

        string line =
            $"{fighterStats.NAME};" +
            $"{fighterStats.WALKING_SPEED};" +
            $"{fighterStats.HORIZONTAL_GROUND_DECELERATION};" +
            $"{fighterStats.FALL_SPEED};" +
            $"{fighterStats.FALL_GRAVITY_ACCELERATION};" +
            $"{fighterStats.AIR_DRIFT_SPEED};" +
            $"{fighterStats.AIR_DRIFT_ACCELERATION}" +
            $"{fighterStats.SHORTJUMP_JUMP_SPEED};" +
            $"{fighterStats.SHORTJUMP_AIR_DECELERATION};" +
            $"{fighterStats.FULLJUMP_JUMP_SPEED};" +
            $"{fighterStats.FULLJUMP_AIR_DECELERATION};" +
            $"{fighterStats.AIRJUMP_JUMP_SPEED};" +
            $"{fighterStats.AIRJUMP_AIR_DECELERATION};" +
            $"{fighterStats.MAX_AIR_JUMP_COUNT};";
        lines.Add(line);


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
    public static ParameterTable Open()
    {
        ParameterTable mchEditorWindow = EditorWindow.GetWindow<ParameterTable>(
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
        float clomunsMinWidth = 20.0f;
        float columnsMaxWidth = 150.0f;
        _columns = new MultiColumnHeaderState.Column[]
        {
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "Name"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "WALKING_SPEED"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "HORIZONTAL_GROUND_DECELERATION"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "FALL_SPEED"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "FALL_GRAVITY_ACCELERATION"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "AIR_DRIFT_SPEED"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "AIR_DRIFT_ACCELERATION"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "SHORTJUMP_JUMP_SPEED"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "SHORTJUMP_AIR_DECELERATION"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "FULLJUMP_JUMP_SPEED"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "FULLJUMP_AIR_DECELERATION"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "AIRJUMP_JUMP_SPEED"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "AIRJUMP_AIR_DECELERATION"),
            CreateMultiColumnHeraderState_Column(clomunsMinWidth,columnsMaxWidth,name: "MAX_AIR_JUMP_COUNT"),
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

    private static MultiColumnHeaderState.Column CreateMultiColumnHeraderState_Column(float minWidth, float maxWidth, string name)
    {
        return new MultiColumnHeaderState.Column()
        {
            allowToggleVisibility = true,
            autoResize = true,
            minWidth = minWidth,
            maxWidth = maxWidth,
            canSort = false,
            sortingArrowAlignment = TextAlignment.Right,
            headerContent = new GUIContent(name),
            headerTextAlignment = TextAlignment.Center,
        };
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

        float heightJump = columnHeight;
        float rowHeight = 20.0f;

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
                width = Mathf.Max(_multiColumnHeaderState.widthOfAllVisibleColumns, positionalRectAreaOfScrollView.width),
                height = Mathf.Max(columnHeight, positionalRectAreaOfScrollView.height)
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


                // fill columns in each row
                FighterStats currentCharacter = fighterStats;

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
                        label: new GUIContent(currentCharacter.NAME),
                        style: nameFieldGUIStyle
                    );
                }
                // WALKING_SPEED  field
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
                    currentCharacter.WALKING_SPEED = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.WALKING_SPEED,
                        style: fieldStyle
                    );
                }
                // HORIZONTAL_GROUND_DECELERATION field
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
                    currentCharacter.HORIZONTAL_GROUND_DECELERATION = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.HORIZONTAL_GROUND_DECELERATION,
                        style: fieldStyle
                    );
                }
                // FALL_SPEED field
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
                    currentCharacter.FALL_SPEED = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.FALL_SPEED,
                        style: fieldStyle
                    );
                }
                // FALL_GRAVITY_ACCELERATION field
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
                    currentCharacter.FALL_GRAVITY_ACCELERATION = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.FALL_GRAVITY_ACCELERATION,
                        style: fieldStyle
                    );
                }
                // AIR_DRIFT_SPEED field
                columnIndex = 5;
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
                    currentCharacter.AIR_DRIFT_SPEED = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.AIR_DRIFT_SPEED,
                        style: fieldStyle
                    );
                }
                // AIR_DRIFT_ACCELERATION field
                columnIndex = 6;
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
                    currentCharacter.AIR_DRIFT_ACCELERATION = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.AIR_DRIFT_ACCELERATION,
                        style: fieldStyle
                    );
                }
                // SHORTJUMP_JUMP_SPEED field
                columnIndex = 7;
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
                    currentCharacter.SHORTJUMP_JUMP_SPEED = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.SHORTJUMP_JUMP_SPEED,
                        style: fieldStyle
                    );
                }
                // SHORTJUMP_AIR_DECELERATION field
                columnIndex = 8;
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
                    currentCharacter.SHORTJUMP_AIR_DECELERATION = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.SHORTJUMP_AIR_DECELERATION,
                        style: fieldStyle
                    );
                }
                // FULLJUMP_JUMP_SPEED field
                columnIndex = 9;
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
                    currentCharacter.FULLJUMP_JUMP_SPEED = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.FULLJUMP_JUMP_SPEED,
                        style: fieldStyle
                    );
                }
                // FULLJUMP_AIR_DECELERATION field
                columnIndex = 10;
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
                    currentCharacter.FULLJUMP_AIR_DECELERATION = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.FULLJUMP_AIR_DECELERATION,
                        style: fieldStyle
                    );
                }
                // AIRJUMP_JUMP_SPEED field
                columnIndex = 11;
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
                    currentCharacter.AIRJUMP_JUMP_SPEED = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.AIRJUMP_JUMP_SPEED,
                        style: fieldStyle
                    );
                }
                // AIRJUMP_AIR_DECELERATION field
                columnIndex = 12;
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
                    currentCharacter.AIRJUMP_AIR_DECELERATION = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.AIRJUMP_AIR_DECELERATION,
                        style: fieldStyle
                    );
                }
                // MAX_AIR_JUMP_COUNT field
                columnIndex = 13;
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
                    currentCharacter.MAX_AIR_JUMP_COUNT = EditorGUI.FloatField(
                        position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                        value: currentCharacter.MAX_AIR_JUMP_COUNT,
                        style: fieldStyle
                    );
                }
                int buttonCount = 0;
                // save button field
                buttonCount = 1;
                Rect saveButtonRect = new Rect(0, rowRect.y + heightJump + rowHeight * buttonCount, _multiColumnHeaderWidth, rowHeight);
                GUILayout.BeginArea(saveButtonRect);
                if (GUILayout.Button("Save to csv-file"))
                {
                    SaveToCSV();
                }
                GUILayout.EndArea();
                // reset button field
                buttonCount++;
                Rect resetButtonRect = new Rect(0, rowRect.y + heightJump + rowHeight * buttonCount, _multiColumnHeaderWidth, rowHeight);
                GUILayout.BeginArea(resetButtonRect);
                if (GUILayout.Button("Reset Values"))
                {
                    ReadCSVFile();
                }
                GUILayout.EndArea();

                // use saved height for each row to draw rectangles to prevent overlapping drawing
                Rect backgroundColorRect = new Rect(source: rowRect)
                {
                    y = rowRect.y + heightJump,
                    height = rowHeight
                };

                // draw a texture before drawing each of the fields for the whole row.
                //if (a % 2 == 0)
                //    EditorGUI.DrawRect(rect: backgroundColorRect, color: _darkerColor);
                //else
                EditorGUI.DrawRect(rect: backgroundColorRect, color: _lighterColor);

                heightJump += rowHeight;

                //// if an element of the tool table has changed, update the scene
                //if (GUI.changed)
                //{
                //    UpStatsOfCharInScene(currentCharacter);
                //}

            }
            GUI.EndScrollView(handleScrollWheel: true);
        }
        GUI.EndGroup();

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

    //private static void UpStatsOfCharInScene(CharacterStats currentCharacter)
    //{
    //    GameObject currChar = GameObject.Find(currentCharacter.NAME);
    //    if(currChar != null)
    //    {
    //        currChar.transform.localScale = new Vector3(currentCharacter.HORIZONTAL_GROUND_DECELERATION, currentCharacter.HORIZONTAL_GROUND_DECELERATION, currentCharacter.HORIZONTAL_GROUND_DECELERATION);
    //        currChar.transform.position = new Vector3(currentCharacter.FALL_SPEED, currentCharacter.FALL_GRAVITY_ACCELERATION, currentCharacter.AIR_DRIFT_SPEED);
    //    }
    //    else
    //    {
    //        Debug.Log(currentCharacter.NAME + " has not been found in the scene");
    //    }
    //}

    private void Awake()
    {
        Initialize();
    }
}
#endif