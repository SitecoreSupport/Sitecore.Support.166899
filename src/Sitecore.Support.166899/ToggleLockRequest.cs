using System;
using Sitecore.Data.Items;
using Sitecore.ExperienceEditor.Speak.Server.Contexts;
using Sitecore.ExperienceEditor.Speak.Server.Requests;
using Sitecore.ExperienceEditor.Speak.Server.Responses;
using Sitecore.Web;
using Sitecore.ExperienceEditor.Speak.Ribbon.Requests.LockItem;
using Sitecore.Links;

namespace Sitecore.Support.ExperienceEditor.Speak.Ribbon.Requests.LockItem
{
    public class ToggleLockRequest : PipelineProcessorRequest<ItemContext>
    {
        public override PipelineProcessorResponseValue ProcessRequest()
        {
            base.RequestContext.ValidateContextItem();
            Sitecore.Data.Items.Item item = this.SwitchLock(base.RequestContext.Item);
            this.HandleVersionCreating(item);
            return new PipelineProcessorResponseValue
            {
                Value = new
                {
                    Locked = item.Locking.IsLocked(),
                    Version = item.Version.Number
                }
            };
        }

        protected Sitecore.Data.Items.Item SwitchLock(Sitecore.Data.Items.Item item)
        {
            if (item.Locking.IsLocked())
            {
                ItemLink[] itemLinks = item.Links.GetValidLinks();
                item.Locking.Unlock();
                foreach (ItemLink link in itemLinks)
                {
                    Item targetItem = link.GetTargetItem();
                    if (targetItem == null) continue;
                    Item langSpecItem = targetItem.Database.GetItem(targetItem.ID, item.Language);
                    if (langSpecItem == null || !langSpecItem.Locking.IsLocked()) continue;
                    langSpecItem.Locking.Unlock();
                }
                return item;
            }
            if (Sitecore.Context.User.IsAdministrator)
            {
                item.Locking.Lock();
                return item;
            }
            return Sitecore.Context.Workflow.StartEditing(item);
        }

        private void HandleVersionCreating(Sitecore.Data.Items.Item finalItem)
        {
            if (base.RequestContext.Item.Version.Number != finalItem.Version.Number)
            {
                Sitecore.Web.WebUtil.SetCookieValue(base.RequestContext.Site.GetCookieKey("sc_date"), string.Empty, DateTime.MinValue);
            }
        }
    }
}