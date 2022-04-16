using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !NET40
using System.ComponentModel.DataAnnotations;
#endif

namespace Anno.EngineData
{
    public static class ValidatetionExtension
    {
        public static ValidResult IsValid<T>(this T value) where T : class
        {
            ValidResult result = new ValidResult();
            try
            {
#if !NET40
                var validationContext = new ValidationContext(value);
                var results = new List<ValidationResult>();
                var isValid = Validator.TryValidateObject(value, validationContext, results, true);

                if (!isValid)
                {
                    result.IsVaild = false;
                    result.ErrorMembers = new List<ErrorMember>();
                    foreach (var item in results)
                    {
                        result.ErrorMembers.Add(new ErrorMember()
                        {
                            ErrorMessage = item.ErrorMessage,
                            ErrorMemberName = string.Join(",", item.MemberNames)
                        });
                    }
                }
                else
#endif
                {
                    result.IsVaild = true;
                }
            }
            catch (Exception ex)
            {
                result.IsVaild = false;
                result.ErrorMembers = new List<ErrorMember>();
                result.ErrorMembers.Add(new ErrorMember()
                {
                    ErrorMessage = ex.Message,
                    ErrorMemberName = "Internal error"
                });
            }

            return result;
        }
    }
    public struct ValidResult
    {
        public List<ErrorMember> ErrorMembers { get; set; }
        public bool IsVaild { get; set; }
    }

    public class ErrorMember
    {
        public string ErrorMessage { get; set; }
        public string ErrorMemberName { get; set; }
    }
}
