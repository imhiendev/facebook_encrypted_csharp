using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LoginWWW
{
    internal class TokenService
    {
        static string url = "https://www.facebook.com/dialog/oauth?scope=user_about_me,user_actions.books,user_actions.fitness,user_actions.music,user_actions.news,user_actions.video,user_activities,user_birthday,user_education_history,user_events,user_friends,user_games_activity,user_groups,user_hometown,user_interests,user_likes,user_location,user_managed_groups,user_photos,user_posts,user_relationship_details,user_relationships,user_religion_politics,user_status,user_tagged_places,user_videos,user_website,user_work_history,email,manage_notifications,manage_pages,publish_actions,publish_pages,read_friendlists,read_insights,read_page_mailboxes,read_stream,rsvp_event,read_mailbox&response_type=token&client_id=124024574287414&redirect_uri=fb124024574287414://authorize/&sso_key=com&display=&fbclid=IwAR1KPwp2DVh2Cu7KdeANz-dRC_wYNjjHk5nR5F-BzGGj7-gTnKimAmeg08k";
        public static string? GrantPermisson(string cookie)
        {
            RequestXNet _request = new RequestXNet(cookie);

            FieldsToken fieldsToken;
            try
            {
                string? resFields = _request.RequestGet(url).Replace("\\\"", "\"");
                fieldsToken = new FieldsToken()
                {
                    jazoest = Regex.Match(resFields, "name=\"jazoest\" value=\"(.*?)\"").Groups[1].Value,
                    fb_dtsg = Regex.Match(resFields, "name=\"fb_dtsg\" value=\"(.*?)\"").Groups[1].Value,
                    scope = Regex.Match(resFields, "name=\"scope\" value=\"(.*?)\"").Groups[1].Value,
                    logger_id = Regex.Match(resFields, "name=\"logger_id\" value=\"(.*?)\"").Groups[1].Value,
                    encrypted_post_body = Regex.Match(resFields, "name=\"encrypted_post_body\" value=\"(.*?)\"").Groups[1].Value
                };
            }
            catch (Exception)
            {

                return null;
            }

            #region POST GRANTPERMISSON
            
            _request.AddParam("referer", url);
            _request.AddParam("jazoest", fieldsToken.jazoest);
            _request.AddParam("fb_dtsg", fieldsToken.fb_dtsg);
            _request.AddParam("from_post", "1");
            _request.AddParam("__CONFIRM__", "1");
            _request.AddParam("scope", fieldsToken.scope);
            _request.AddParam("logger_id", fieldsToken.logger_id);
            _request.AddParam("encrypted_post_body", fieldsToken.encrypted_post_body);
            _request.AddParam("return_format[]", "access_token");
            #endregion
            var result = _request.RequestPost("https://www.facebook.com/v1.0/dialog/oauth/skip/submit/");
            if (result == "error")
                return null;
            if (result.Contains("access_token"))
                return Regex.Match(result, "access_token=(.*?)&").Groups[1].Value;
            else
                return null;
        }
        public class FieldsToken
        {
            public string? jazoest { get; set; }
            public string? fb_dtsg { get; set; }
            public string? scope { get; set; }
            public string? logger_id { get; set; }
            public string? encrypted_post_body { get; set; }
        }
    }
}
