using Microsoft.Practices.Prism.StoreApps;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynoDL8.Model
{
    public class Credentials : ValidatableBindableBase, IEquatable<Credentials>
    {
        private string hostname;
        private string user;
        private string password;

        [CustomValidation(typeof(Credentials), "ValidateHostname")]
        [Required]
        public string Hostname
        {
            get { return this.hostname; }
            set { this.SetProperty(ref this.hostname, value); }
        }

        [Required]
        public string User
        {
            get { return this.user; }
            set { this.SetProperty(ref this.user, value); }
        }

        [Required]
        public string Password
        {
            get { return this.password; }
            set { this.SetProperty(ref this.password, value); }
        }

        public static ValidationResult ValidateHostname(object value, ValidationContext validationContext)
        {
            bool isValid = false;
            try
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (validationContext == null)
                {
                    throw new ArgumentNullException("validationContext");
                }

                var credentials = (Credentials)validationContext.ObjectInstance;
                string hostname = credentials.Hostname;

                Uri uri = null;
                string formatError = null;
                try
                {
                    uri = new Uri(hostname);
                }
                catch (FormatException e)
                {
                    formatError = e.Message;
                }
                if (formatError != null)
                {
                    return new ValidationResult(formatError);
                }

                if (uri != null)
                {
                    if (!(hostname.StartsWith(@"http://") || hostname.StartsWith(@"https://")))
                    {
                        return new ValidationResult("Host name should start with either http:// or https://");
                    }
                    else if (uri.PathAndQuery != @"/")
                    {
                        return new ValidationResult("Host name should not contain a path or qeury");
                    }
                }

                isValid = true;
            }
            catch(Exception e)
            {
                isValid = false;
                return new ValidationResult(e.Message);
            }

            if (isValid)
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult("Error in hostname");
            }
        }

        public bool Equals(Credentials other)
        {
            return other != null &&
                string.Equals(other.Hostname, this.Hostname, StringComparison.Ordinal) &&
                string.Equals(other.User, this.User, StringComparison.Ordinal) &&
                string.Equals(other.Password, this.Password, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as Credentials);
        }

        public override int GetHashCode()
        {
            return this.hostname.GetHashCode() ^ this.user.GetHashCode() ^ this.password.GetHashCode();
        }

        public static HashSet<Credentials> EnumerableFromJason(string allCredentialsJason)
        {
            return JsonConvert.DeserializeObject<HashSet<Credentials>>(allCredentialsJason);
        }

        public static Credentials FromJason(string credentialsJason)
        {
            return JsonConvert.DeserializeObject<Credentials>(credentialsJason);
        }

        public string ToJason()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static string ToJason(HashSet<Credentials> credentials)
        {
            return JsonConvert.SerializeObject(credentials);
        }

        public Credentials Clone()
        {
            return new Credentials()
            {
                hostname = this.hostname,
                user = this.user,
                password = this.password,
            };
        }
    }
}
