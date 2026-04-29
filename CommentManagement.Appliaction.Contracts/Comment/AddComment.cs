using _0_Framework.Application;
using System.ComponentModel.DataAnnotations;

namespace CommentManagement.Application.Contracts.Comment
{
    /// <summary>
    /// Represents the data required to add a new comment.
    /// </summary>
    public class AddComment
    {
        /// <summary>
        /// The text content of the comment. This can be optional.
        /// </summary>
        /// <example>This is a great product! Highly recommended.</example>
        public string? Message { get; set; }

        /// <summary>
        /// The ID of the entity (e.g., product, article) to which this comment is being added.
        /// </summary>
        /// <example>101</example>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public long RecordId { get; set; }

        /// <summary>
        /// The type of the entity being commented on. For example, 1 for Product, 2 for Article.
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        public int Type { get; set; }

        /// <summary>
        /// The user's rating for the entity, on a scale of 1 to 5.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = ValidationMessages.IsRequired)]
        [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ باشد.")]
        public int Rate { get; set; }
    }
}
