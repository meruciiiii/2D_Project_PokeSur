using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public interface IAttackInterface
{
    // 공격 로직을 실행하는 핵심 메서드입니다.
    /*
     이 스크립트는 "모든 포켓몬 공격기는 
    ExecuteAttack()이라는 이름의 메서드를 반드시 가지고 있어야 한다"
    고 강제하는 규칙입니다.

    나중에 PichuAttack이든 ChikoritaAttack이든 
    이 인터페이스를 상속받아 ExecuteAttack()을 구현하게 됩니다.
     */
    void ExecuteAttack();
}
