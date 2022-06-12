using Colin.Common.Code.Physics.Dynamics;

namespace Colin.Common.Code.Physics.Collision.ContactSystem
{
    /// <summary>
    /// 接触边用于在接触图中将实体和接触连接在一起，其中每个实体都是一个节点
    /// 每个触点都是一条边。接触边属于在每个附着的实体中维护的双链接列表。每个
    /// contact有两个接触节点，每个连接的实体对应一个。
    /// </summary>
    public sealed class ContactEdge
    {
        /// <summary>The contact</summary>
        public Contact? Contact;

        /// <summary>The next contact edge in the body's contact list</summary>
        public ContactEdge? Next;

        /// <summary>Provides quick access to the other body attached.</summary>
        public Body? Other;

        /// <summary>The previous contact edge in the body's contact list</summary>
        public ContactEdge? Prev;
    }
}