using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poolable : MonoBehaviour
{
    // 이 스크립트 컴포넌트를 갖고 있는 애들만 object pooling이 가능하도록 함.
    public bool isUsing;
    public bool isMercenary;
}
