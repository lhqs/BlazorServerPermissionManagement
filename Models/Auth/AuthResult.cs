namespace ManagerSystem.Models.Auth
{
    /// <summary>
    /// 认证结果模型
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}