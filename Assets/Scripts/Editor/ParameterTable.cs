using System.Collections.Generic;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;

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
            fighterStats.NAME                           = values[0];
            fighterStats.WALKING_SPEED                  = float.Parse(values[1]);
            fighterStats.HORIZONTAL_GROUND_DECELERATION = float.Parse(values[2]);
            fighterStats.FALL_SPEED                     = float.Parse(values[3]);
            fighterStats.FALL_GRAVITY_ACCELERATION      = float.Parse(values[4]);
            fighterStats.AIR_DRIFT_SPEED                = float.Parse(values[5]);
            fighterStats.AIR_DRIFT_ACCELERATION         = float.Parse(values[6]);
            fighterStats.SHORTJUMP_JUMP_SPEED           = float.Parse(values[7]);
            fighterStats.SHORTJUMP_AIR_DECELERATION     = float.Parse(values[8]);
            fighterStats.FULLJUMP_JUMP_SPEED            = float.Parse(values[9]);
            fighterStats.FULLJUMP_AIR_DECELERATION      = float.Parse(values[10]);
            fighterStats.AIRJUMP_JUMP_SPEED             = float.Parse(values[11]);
            fighterStats.AIRJUMP_AIR_DECELERATION       = float.Parse(values[12]);
            fighterStats.MAX_AIR_JUMP_COUNT             = float.Parse(values[13]);
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
            $"{fighterStats.AIR_DRIFT_ACCELERATION};" +
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
        string relativeCSVFilePath = "Resources/CSV-Files/stats.csv";
        return Path.Combine(Application.dataPath, relativeCSVFilePath);
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
    //private bool _firstOnGUIIterationAfterInitialize; // used for loop if there are more than one characterStats row

    private void Initialize()
    {

        //_firstOnGUIIterationAfterInitialize = true; // used for loop if there are more than one characterStats row
        _multiColumnHeaderWidth = position.width;
        float columnsMinWidth = 20.0f;
        float columnsMaxWidth = 150.0f;
        string[] columnNames = new[]
        {
            "Name",
            "WALKING_SPEED",
            "HORIZONTAL_GROUND_DECELERATION",
            "FALL_SPEED",
            "FALL_GRAVITY_ACCELERATION",
            "AIR_DRIFT_SPEED",
            "AIR_DRIFT_ACCELERATION",
            "SHORTJUMP_JUMP_SPEED",
            "SHORTJUMP_AIR_DECELERATION",
            "FULLJUMP_JUMP_SPEED",
            "FULLJUMP_AIR_DECELERATION",
            "AIRJUMP_JUMP_SPEED",
            "AIRJUMP_AIR_DECELERATION",
            "MAX_AIR_JUMP_COUNT",
        };

        _columns = new MultiColumnHeaderState.Column[columnNames.Length];
        for (int i = 0; i < columnNames.Length; i++)
        {
            _columns[i] = CreateMultiColumnHeaderState_Column(columnsMinWidth, columnsMaxWidth, name: columnNames[i]);
        }

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

    private static MultiColumnHeaderState.Column CreateMultiColumnHeaderState_Column(float minWidth, float maxWidth, string name)
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
        if (_multiColumnHeader == null) Initialize();
        // makes table background more greyish
        GUIStyle groupGUIStyle = new GUIStyle(GUI.skin.box);
        Vector2 groupRectPaddingInWindow;
        Rect groupRect;
        CreatePseudoPadding(windowRect, out groupRectPaddingInWindow, out groupRect); // actually the size of the rectangle

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

                rowRect = FillTableWithFighterStats(columnHeight, currentCharacter, rowRect);
                rowRect = CreateButtons(columnHeight, rowHeight, rowRect);

                // use saved height for each row to draw rectangles to prevent overlapping drawing
                Rect backgroundColorRect = new Rect(source: rowRect)
                {
                    y = rowRect.y + columnHeight,
                    height = rowHeight
                };
            }
            GUI.EndScrollView(handleScrollWheel: true);
        }
        GUI.EndGroup();

        //_firstOnGUIIterationAfterInitialize = false; // used for loop if there are more than one characterStats row
    }

    private static void CreatePseudoPadding(Rect windowRect, out Vector2 groupRectPaddingInWindow, out Rect groupRect)
    {
        groupRectPaddingInWindow = new Vector2(10.0f, 10.0f);
        groupRect = new Rect(source: windowRect);
        groupRect.x += groupRectPaddingInWindow.x;
        groupRect.y += groupRectPaddingInWindow.y;
        groupRect.width -= groupRectPaddingInWindow.x * 2;
        groupRect.height -= groupRectPaddingInWindow.y * 2;
    }

    private Rect CreateButtons(float heightJump, float rowHeight, Rect rowRect)
    {
        int buttonCount = 0;
        // save button field
        buttonCount++;
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
        return rowRect;
    }

    private Rect FillTableWithFighterStats(float heightJump, FighterStats currentCharacter, Rect rowRect)
    {
        // name field
        int columnIndex = 0;
        rowRect = CreateStringFieldOfTable(heightJump, ref currentCharacter.NAME, rowRect, columnIndex);
        // WALKING_SPEED field
        columnIndex = 1;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.WALKING_SPEED, rowRect, columnIndex);
        // HORIZONTAL_GROUND_DECELERATION field
        columnIndex = 2;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.HORIZONTAL_GROUND_DECELERATION, rowRect, columnIndex);
        // FALL_SPEED field
        columnIndex = 3;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.FALL_SPEED, rowRect, columnIndex);
        // FALL_GRAVITY_ACCELERATION field
        columnIndex = 4;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.FALL_GRAVITY_ACCELERATION, rowRect, columnIndex);
        // AIR_DRIFT_SPEED field
        columnIndex = 5;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.AIR_DRIFT_SPEED, rowRect, columnIndex);
        // AIR_DRIFT_ACCELERATION field
        columnIndex = 6;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.AIR_DRIFT_ACCELERATION, rowRect, columnIndex);
        // SHORTJUMP_JUMP_SPEED field
        columnIndex = 7;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.SHORTJUMP_JUMP_SPEED, rowRect, columnIndex);
        // SHORTJUMP_AIR_DECELERATION field
        columnIndex = 8;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.SHORTJUMP_AIR_DECELERATION, rowRect, columnIndex);
        // FULLJUMP_JUMP_SPEED field
        columnIndex = 9;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.FULLJUMP_JUMP_SPEED, rowRect, columnIndex);
        // FULLJUMP_AIR_DECELERATION field
        columnIndex = 10;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.FULLJUMP_AIR_DECELERATION, rowRect, columnIndex);
        // AIRJUMP_JUMP_SPEED field
        columnIndex = 11;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.AIRJUMP_JUMP_SPEED, rowRect, columnIndex);
        // AIRJUMP_AIR_DECELERATION field
        columnIndex = 12;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.AIRJUMP_AIR_DECELERATION, rowRect, columnIndex);
        // MAX_AIR_JUMP_COUNT field
        columnIndex = 13;
        rowRect = CreateFloatFieldOfTable(heightJump, ref currentCharacter.MAX_AIR_JUMP_COUNT, rowRect, columnIndex);
        return rowRect;
    }

    private Rect CreateFloatFieldOfTable(float heightJump, ref float currentCharacterAttribute, Rect rowRect, int columnIndex)
    {
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
            currentCharacterAttribute = EditorGUI.FloatField(
                position: _multiColumnHeader.GetCellRect(visibleColumnIndex: visibleColumnIndex, columnRect),
                value: currentCharacterAttribute,
                style: fieldStyle
            );
        }

        return rowRect;
    }

    private Rect CreateStringFieldOfTable(float heightJump, ref string currentCharacterAttribute, Rect rowRect, int columnIndex)
    {
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
                label: new GUIContent(currentCharacterAttribute),
                style: nameFieldGUIStyle
            );
        }

        return rowRect;
    }

    private void Awake()
    {
        Initialize();
    }
}
#endif