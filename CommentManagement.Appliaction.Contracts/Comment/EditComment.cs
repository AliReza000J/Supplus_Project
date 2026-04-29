using System.ComponentModel.DataAnnotations;

namespace CommentManagement.Application.Contracts.Comment
{
    /// <summary>
    /// Represents the data required to edit an existing comment.
    /// </summary>
    public class EditComment
    {
        /// <summary>
        /// The unique identifier of the comment to be edited.
        /// </summary>
        /// <example>78</example>
        [Required]
        public long Id { get; set; }

        /// <summary>
        /// The new text content for the comment. Can be null if only the rate is being updated.
        /// </summary>
        /// <example>I'd like to update my previous comment...</example>
        public string? Message { get; set; }

        /// <summary>
        /// The new rating for the entity, on a scale of 1 to 5. Can be null if only the message is being updated.
        /// </summary>
        /// <example>4</example>
        [Range(1, 5, ErrorMessage = "امتیاز باید بین ۱ تا ۵ باشد.")]
        public int? Rate { get; set; }
    }
}
