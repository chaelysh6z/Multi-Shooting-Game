/// <summary>
/// 오브젝트 풀에서 관리되는 프리팹 타입을 정의
/// </summary>
public enum PoolType
{
    EnemyB,         // 보스 적
    EnemyL,         // 대형 적
    EnemyM,         // 중형 적
    EnemyS,         // 소형 적
    ItemCoin,       // 코인 아이템
    ItemPower,      // 파워업 아이템
    ItemBoom,       // 폭탄 아이템
    BulletPlayerA,  // 플레이어 기본 총알
    BulletPlayerB,  // 플레이어 강화 총알
    BulletEnemyA,   // 적 총알 A
    BulletEnemyB,   // 적 총알 B
    BulletFollower, // 플레이어 팔로워 총알
    BulletBossA,    // 보스 패턴 총알 A
    BulletBossB,    // 보스 패턴 총알 B
    Explosion       // 폭발 이펙트
}