using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions; // これが必要です

public class FirebaseTest : MonoBehaviour
{
    DatabaseReference reference;

    void Start()
    {
        // 1. Firebaseの準備をする（おまじない）
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available) {
                
                // 準備OK！
                InitializeFirebase();
                
            } else {
                Debug.LogError("Firebaseが使えません: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        // データベースの「ルート（根っこ）」を取得
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        // テスト書き込み：「message」という場所に「Hello!」と書く
        reference.Child("message").SetValueAsync("Hello Firebase!")
            .ContinueWithOnMainThread(task => {
                if (task.IsCompleted) {
                    Debug.Log("書き込み成功！Firebaseコンソールを見てみて！");
                }
            });
    }
}
