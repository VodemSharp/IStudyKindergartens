﻿using IStudyKindergartens.Models;
using IStudyKindergartens.Models.Users;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IStudyKindergartens.Repositories
{
    public interface ISiteUserManager
    {
        IEnumerable<SiteUser> GetSiteUsers();
        SiteUser GetSiteUserById(string id);
        string GetPictureUIDById(string id);
        IEnumerable<SiteUser> GetContactUsers(string id);
        List<AddContactListItemViewModel> GetAddContactListViewModel(string id);

        void AddSiteUser(AddUserViewModel model, string userId, HttpServerUtilityBase Server);
        void AddContactUser(string id, string contactUserId);
        
        void EditSiteUser(EditUserViewModel model, string userId, HttpServerUtilityBase Server);

        void DeleteSiteUser(string userId, HttpServerUtilityBase server);
        void RemoveContactUser(string id, string contactUserId);

        void RegisterSiteUser(SiteUser siteUser);
        bool SwitchAddRemoveKindergartenForSiteUser(string KindergartenId, string userId);
    }

    public class SiteUserManager : IDisposable, ISiteUserManager
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        #region Help

        public void RegisterSiteUser(SiteUser siteUser)
        {
            db.SiteUsers.Add(siteUser);
            db.SaveChanges();
        }

        public bool SwitchAddRemoveKindergartenForSiteUser(string KindergartenId, string userId)
        {
            if (db.SiteUserKindergartens.Any(suk => suk.KindergartenId == KindergartenId && suk.SiteUserId == userId))
            {
                db.SiteUserKindergartens.Remove(db.SiteUserKindergartens.Where(suk => suk.KindergartenId == KindergartenId && suk.SiteUserId == userId).First());
                db.SaveChanges();
                return false;
            }
            else
            {
                db.SiteUserKindergartens.Add(new SiteUserKindergarten { SiteUserId = userId, KindergartenId = KindergartenId });
                db.SaveChanges();
                return true;
            }
        }

        #endregion

        #region GetSomething

        public IEnumerable<SiteUser> GetSiteUsers()
        {
            return db.SiteUsers.ToList();
        }

        public SiteUser GetSiteUserById(string id)
        {
            SiteUser siteUser = null;
            try
            {
                siteUser = db.SiteUsers.Where(su => su.Id == id).First();
            }
            catch (Exception) { }
            return siteUser;
        }

        public string GetPictureUIDById(string id)
        {
            return db.SiteUserClaims.Where(suc => suc.SiteUserId == id && suc.ClaimType.Type == "Picture").First().ClaimValue;
        }

        public IEnumerable<SiteUser> GetContactUsers(string id)
        {
            List<SiteUser> siteUsers = new List<SiteUser>();
            List<SiteUserContact> siteUserContacts = db.SiteUserContacts.Where(suc => suc.SiteUserId == id).ToList();
            Contact tempContact;
            SiteUserContact tempSiteUserContact;
            for (int i = 0; i < siteUserContacts.Count; i++)
            {
                tempSiteUserContact = siteUserContacts[i];
                tempContact = db.Contacts.Where(c => c.Id == tempSiteUserContact.ContactId).First();
                siteUsers.Add(db.SiteUsers.Where(su => su.Id == tempContact.SiteUserId).First());
            }
            return siteUsers;
        }

        public List<AddContactListItemViewModel> GetAddContactListViewModel(string id)
        {
            List<AddContactListItemViewModel> model = new List<AddContactListItemViewModel>();
            List<SiteUser> allSiteUsers = GetSiteUsers().ToList();
            List<SiteUser> addedSiteUsers = GetContactUsers(id).ToList();
            bool isAdded;
            for (int i = 0; i < allSiteUsers.Count; i++)
            {
                isAdded = false;
                for (int j = 0; j < addedSiteUsers.Count; j++)
                {
                    if (allSiteUsers[i] == addedSiteUsers[j])
                    {
                        isAdded = true;
                        break;
                    }
                }
                model.Add(new AddContactListItemViewModel { SiteUser = allSiteUsers[i], IsAdded = isAdded });
            }
            return model;
        }

        #endregion

        #region AddSomething

        private void AddPictureClaim(string id, string pictureName, HttpServerUtilityBase server)
        {
            ClaimType claimType;
            try
            {
                claimType = db.ClaimTypes.Where(c => c.Type == "Picture").First();
            }
            catch (Exception)
            {
                claimType = db.ClaimTypes.Add(new ClaimType { Type = "Picture" });
            }
            db.SiteUserClaims.Add(new SiteUserClaim { ClaimTypeId = claimType.Id, SiteUserId = id, ClaimValue = pictureName });
            System.IO.File.Copy(server.MapPath("~/Images/Uploaded/Temp/" + pictureName), server.MapPath("~/Images/Uploaded/Source/" + pictureName));
            System.IO.File.Delete(server.MapPath("~/Images/Uploaded/Temp/" + pictureName));
        }

        public void AddSiteUser(AddUserViewModel model, string userId, HttpServerUtilityBase server)
        {
            SiteUser siteUser = new SiteUser { Name = model.Name, Surname = model.Surname, FathersName = model.FathersName, DateOfBirth = model.DateOfBirth, Id = userId };
            db.SiteUsers.Add(siteUser);
            if (model.PictureName != null)
            {
                AddPictureClaim(userId, model.PictureName, server);
            }
            db.SaveChanges();
        }

        public void AddContactUser(string id, string contactUserId)
        {
            Contact contact = db.Contacts.Add(new Contact { SiteUserId = contactUserId });
            db.SiteUserContacts.Add(new SiteUserContact { ContactId = contact.Id, SiteUserId = id });
            db.SaveChanges();
        }

        #endregion

        #region EditSomething

        public void EditSiteUser(EditUserViewModel model, string userId, HttpServerUtilityBase server)
        {
            SiteUser siteUser = db.SiteUsers.Where(su => su.Id == userId).First();
            siteUser.Name = model.Name;
            siteUser.Surname = model.Surname;
            siteUser.FathersName = model.FathersName;
            siteUser.ApplicationUser.Email = model.Email;
            siteUser.ApplicationUser.PhoneNumber = "+38 " + model.PhoneNumber;
            if (model.PictureName != null)
            {
                SiteUserClaim siteUserClaim;
                try
                {
                    siteUserClaim = db.SiteUserClaims.Where(suc => suc.SiteUserId == userId && suc.ClaimType.Type == "Picture").First();
                    string previosClaimValue = siteUserClaim.ClaimValue;
                    siteUserClaim.ClaimValue = model.PictureName;
                    System.IO.File.Copy(server.MapPath("~/Images/Uploaded/Temp/" + model.PictureName), server.MapPath("~/Images/Uploaded/Source/" + model.PictureName));
                    System.IO.File.Delete(server.MapPath("~/Images/Uploaded/Temp/" + model.PictureName));
                    System.IO.File.Delete(server.MapPath("~/Images/Uploaded/Source/" + previosClaimValue));
                }
                catch (Exception)
                {
                    AddPictureClaim(siteUser.Id, model.PictureName, server);
                }
            }
            else
            {
                try
                {
                    SiteUserClaim siteUserClaim = db.SiteUserClaims.Where(suc => suc.SiteUserId == userId && suc.ClaimType.Type == "Picture").First();
                    System.IO.File.Delete(server.MapPath("~/Images/Uploaded/Source/" + siteUserClaim.ClaimValue));
                    db.SiteUserClaims.Remove(siteUserClaim);
                }
                catch (Exception) { }
            }
            siteUser.DateOfBirth = model.DateOfBirth;
            db.SaveChanges();
        }

        #endregion

        #region DeleteSomething

        public void DeleteSiteUser(string userId, HttpServerUtilityBase server)
        {
            try
            {
                SiteUserClaim siteUserClaim = db.SiteUserClaims.Where(suc => suc.SiteUserId == userId && suc.ClaimType.Type == "Picture").First();
                System.IO.File.Delete(server.MapPath("~/Images/Uploaded/Source/" + siteUserClaim.ClaimValue));
                db.SiteUserClaims.Remove(siteUserClaim);
            }
            catch (Exception) { }
            try
            {
                SiteUser siteUser = db.SiteUsers.Include("ApplicationUser").Where(su => su.Id == userId).First();
                db.SiteUserClaims.RemoveRange(db.SiteUserClaims.Where(suc => suc.SiteUserId == userId));
                db.SiteUserKindergartens.RemoveRange(db.SiteUserKindergartens.Where(suk => suk.SiteUserId == userId));
                List<Rating> userRatings = db.Ratings.Where(r => r.SiteUserId == userId).ToList();
                for (int i = 0; i < userRatings.Count; i++)
                {
                    for (int j = 0; j < userRatings[i].QuestionRatings.Count; j++)
                    {
                        db.QuestionRatings.Remove(userRatings[i].QuestionRatings.ToList()[j]);
                    }
                }
                db.Ratings.RemoveRange(userRatings);
                if (siteUser != null)
                {
                    db.Users.Remove(siteUser.ApplicationUser);
                    db.SiteUsers.Remove(siteUser);
                    db.SaveChanges();
                }
            }
            catch (Exception) { }
        }

        public void RemoveContactUser(string id, string contactUserId)
        {
            List<SiteUserContact> siteUserContacts = db.SiteUserContacts.Where(suc => suc.SiteUserId == id).ToList();
            Contact contact;
            int tempId;
            for (int i = 0; i < siteUserContacts.Count; i++)
            {
                tempId = siteUserContacts[i].ContactId;
                contact = db.Contacts.Where(c => c.Id == tempId).First();
                if (contact.SiteUserId == contactUserId)
                {
                    db.SiteUserContacts.Remove(db.SiteUserContacts.Where(suc => suc.ContactId == tempId).First());
                    db.Contacts.Remove(contact);
                    db.SaveChanges();
                    return;
                }
            }
        }

        #endregion

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}