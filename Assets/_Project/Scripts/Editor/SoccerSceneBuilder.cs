using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 메뉴 Tools > Soccer > Build 1v1 Scene 으로 경기장 씬을 새로 만든다
public static class SoccerSceneBuilder
{
    const string ScenePath = "Assets/_Project/Scenes/Soccer1v1.unity";
    const string MaterialFolder = "Assets/_Project/Materials";
    const float FieldLength = 30f;
    const float FieldWidth = 18f;
    const float GoalWidth = 4f;

    [MenuItem("Tools/Soccer/Build 1v1 Scene")]
    public static void Build()
    {
        EditorSceneManager.SaveOpenScenes();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var grass = GetMaterial("Soccer_Grass", new Color(0.25f, 0.6f, 0.25f));
        var white = GetMaterial("Soccer_White", Color.white);
        var wall = GetMaterial("Soccer_Wall", new Color(0.55f, 0.55f, 0.6f));
        var red = GetMaterial("Soccer_Red", new Color(0.9f, 0.2f, 0.2f));
        var blue = GetMaterial("Soccer_Blue", new Color(0.2f, 0.4f, 0.95f));
        var dark = GetMaterial("Soccer_Dark", new Color(0.1f, 0.1f, 0.1f));

        var field = new GameObject("Field").transform;

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(field);
        ground.transform.localScale = new Vector3(FieldLength / 10f, 1f, FieldWidth / 10f);
        ground.GetComponent<Renderer>().sharedMaterial = grass;

        var centerLine = Box("CenterLine", field, new Vector3(0f, 0.005f, 0f), new Vector3(0.1f, 0.01f, FieldWidth), white);
        Object.DestroyImmediate(centerLine.GetComponent<Collider>());

        // 옆벽과 골대 양옆 끝벽
        float halfL = FieldLength / 2f, halfW = FieldWidth / 2f;
        float endWallLen = halfW - GoalWidth / 2f;
        Box("Wall_Top", field, new Vector3(0f, 0.5f, halfW + 0.25f), new Vector3(FieldLength + 1f, 1f, 0.5f), wall);
        Box("Wall_Bottom", field, new Vector3(0f, 0.5f, -halfW - 0.25f), new Vector3(FieldLength + 1f, 1f, 0.5f), wall);
        foreach (float sx in new[] { -1f, 1f })
            foreach (float sz in new[] { -1f, 1f })
                Box("Wall_End", field, new Vector3(sx * (halfL + 0.25f), 0.5f, sz * (GoalWidth / 2f + endWallLen / 2f)), new Vector3(0.5f, 1f, endWallLen), wall);

        var managerGo = new GameObject("GameManager");
        var manager = managerGo.AddComponent<SoccerGameManager>();

        // RED는 +x 골대를, BLUE는 -x 골대를 공격
        var goalRight = BuildGoal("Goal_Right", field, 1f, white, manager, 0);
        var goalLeft = BuildGoal("Goal_Left", field, -1f, white, manager, 1);

        var ball = BuildBall(white);
        var redPlayer = BuildPlayer("Player_RED", 0, new Vector3(-6f, 1f, 0f), red, dark, ball, goalRight, 6f, 0.7f);
        var bluePlayer = BuildPlayer("Player_BLUE", 1, new Vector3(6f, 1f, 0f), blue, dark, ball, goalLeft, 5.8f, 0.75f);

        redPlayer.opponent = bluePlayer;
        bluePlayer.opponent = redPlayer;
        redPlayer.ownGoal = goalLeft;
        bluePlayer.ownGoal = goalRight;

        manager.ball = ball;
        manager.players = new[] { redPlayer, bluePlayer };
        manager.startPositions = new[] { redPlayer.transform.position, bluePlayer.transform.position };

        var cam = Camera.main;
        cam.transform.SetPositionAndRotation(new Vector3(0f, 17f, -12f), Quaternion.Euler(58f, 0f, 0f));

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings(ScenePath);
        Debug.Log("Soccer 1v1 scene built: " + ScenePath);
    }

    static Transform BuildGoal(string name, Transform parent, float side, Material mat, SoccerGameManager manager, int scoringTeam)
    {
        var goal = new GameObject(name).transform;
        goal.SetParent(parent);
        float halfL = FieldLength / 2f;
        Box("Back", goal, new Vector3(side * (halfL + 1.75f), 1f, 0f), new Vector3(0.5f, 2f, GoalWidth + 1f), mat);
        Box("Post", goal, new Vector3(side * (halfL + 0.75f), 1f, GoalWidth / 2f + 0.25f), new Vector3(1.5f, 2f, 0.5f), mat);
        Box("Post", goal, new Vector3(side * (halfL + 0.75f), 1f, -GoalWidth / 2f - 0.25f), new Vector3(1.5f, 2f, 0.5f), mat);
        Box("Crossbar", goal, new Vector3(side * (halfL + 0.75f), 2.1f, 0f), new Vector3(1.5f, 0.2f, GoalWidth + 1f), mat);

        var trigger = Box("GoalTrigger", goal, new Vector3(side * (halfL + 0.9f), 1f, 0f), new Vector3(1.2f, 2f, GoalWidth), null);
        Object.DestroyImmediate(trigger.GetComponent<MeshRenderer>());
        trigger.GetComponent<Collider>().isTrigger = true;
        var goalScript = trigger.AddComponent<SoccerGoal>();
        goalScript.scoringTeam = scoringTeam;
        goalScript.manager = manager;
        return trigger.transform;
    }

    static SoccerBall BuildBall(Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "Ball";
        go.transform.position = new Vector3(0f, 0.5f, 0f);
        go.transform.localScale = Vector3.one * 0.5f;
        go.GetComponent<Renderer>().sharedMaterial = mat;

        string bouncePath = MaterialFolder + "/BallBounce.physicsMaterial";
        var bounce = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(bouncePath);
        if (bounce == null)
        {
            bounce = new PhysicsMaterial("BallBounce") { bounciness = 0.7f, dynamicFriction = 0.3f, staticFriction = 0.3f, bounceCombine = PhysicsMaterialCombine.Maximum };
            AssetDatabase.CreateAsset(bounce, bouncePath);
        }
        go.GetComponent<Collider>().sharedMaterial = bounce;

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 0.45f;
        rb.linearDamping = 0.4f;
        rb.angularDamping = 0.5f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        return go.AddComponent<SoccerBall>();
    }

    static SoccerPlayer BuildPlayer(string name, int team, Vector3 position, Material body, Material face, SoccerBall ball, Transform opponentGoal, float speed, float accuracy)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = name;
        go.transform.position = position;
        go.transform.rotation = Quaternion.LookRotation(new Vector3(-Mathf.Sign(position.x), 0f, 0f));
        go.GetComponent<Renderer>().sharedMaterial = body;

        // 바라보는 방향 표시용
        var nose = Box("Nose", go.transform, Vector3.zero, Vector3.one * 0.3f, face);
        nose.transform.localPosition = new Vector3(0f, 0.5f, 0.5f);
        Object.DestroyImmediate(nose.GetComponent<Collider>());

        var rb = go.AddComponent<Rigidbody>();
        rb.mass = 70f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var player = go.AddComponent<SoccerPlayer>();
        player.team = team;
        player.ball = ball;
        player.opponentGoal = opponentGoal;
        player.moveSpeed = speed;
        player.accuracy = accuracy;
        return player;
    }

    static GameObject Box(string name, Transform parent, Vector3 position, Vector3 size, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = size;
        if (mat != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static Material GetMaterial(string name, Color color)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    static void AddToBuildSettings(string path)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (scenes.Exists(s => s.path == path)) return;
        scenes.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
