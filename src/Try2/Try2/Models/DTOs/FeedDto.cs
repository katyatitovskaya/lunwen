namespace Try2.Models.DTOs
{
    public class FeedDto
    {
        /// <summary>
        /// Лента всех постов
        /// </summary>
        public List<PostDto> AllPosts { get; set; } = new();

        /// <summary>
        /// Лента постов подписок.
        /// null = пользователь не авторизован
        /// </summary>
        public List<PostDto>? SubscriptionPosts { get; set; }
    }
}
