using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace OpportunityApproval
{
    public class OpportunityApproval : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Retrieve the context of the plugin execution
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Retrieve the tracing service
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Start tracing
            tracingService.Trace("OpportunityApproval plugin started...");

            try
            {
                if (context.MessageName.ToLower() == "create")
                {
                    tracingService.Trace("Handling opportunity creation...");
                    HandleOpportunityCreation(serviceProvider, tracingService, context);
                }
                else if (context.MessageName.ToLower() == "update")
                {
                    tracingService.Trace("Handling opportunity update...");
                    HandleOpportunityUpdate(serviceProvider, tracingService, context);
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions
                tracingService.Trace("An error occurred: {0}", ex.ToString());
                throw;
            }

            // End tracing
            tracingService.Trace("OpportunityApproval plugin completed.");
        }

        private void HandleOpportunityCreation(IServiceProvider serviceProvider, ITracingService tracingService, IPluginExecutionContext context)
        {
            // Retrieve the service factory to create organization service
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            // Create an instance of the organization service
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            Entity opportunityEntity = (Entity)context.InputParameters["Target"];

            tracingService.Trace("Handling opportunity creation...");

            if (opportunityEntity != null && opportunityEntity.Contains("estimatedvalue"))
            {
                decimal minimumThreshold = 10000;
                Money estimateRevenue = (Money)opportunityEntity.Attributes["estimatedvalue"];
                decimal estimateValue = estimateRevenue.Value;

                if (estimateValue < minimumThreshold)
                {
                    // Retrieve the ID of the sales manager user
                    Guid salesManagerUserId = GetSalesManagerUserId(service, tracingService);

                    // Assign the opportunity record to the sales manager user
                    EntityReference salesManagerUserRef = new EntityReference("systemuser", salesManagerUserId);
                    opportunityEntity.Attributes["ownerid"] = salesManagerUserRef;

                    // Update the "Budget Status" field to "Pending Approval"
                    opportunityEntity.Attributes["new_BudgetStatus"] = new OptionSetValue(0);
                }
                else
                {
                    // Update the "Budget Status" field to "Approved"
                    opportunityEntity.Attributes["new_BudgetStatus"] = new OptionSetValue(1);
                }
            }
        }

        //private void HandleOpportunityUpdate(IServiceProvider serviceProvider, ITracingService tracingService, IPluginExecutionContext context)
        //{
        //    // Retrieve the service factory to create organization service
        //    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

        //    // Create an instance of the organization service
        //    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

        //    Entity opportunityEntity = (Entity)context.InputParameters["Target"];

        //    tracingService.Trace("Handling opportunity update...");

        //    if (opportunityEntity != null && opportunityEntity.Contains("estimatedvalue") && context.PreEntityImages.Contains("PreImage"))
        //    {
        //        Money preImageEstimateRevenue = (Money)context.PreEntityImages["PreImage"]["estimatedvalue"];
        //        Money estimateRevenue = (Money)opportunityEntity.Attributes["estimatedvalue"];

        //        // Check if estimate revenue has changed
        //        if (preImageEstimateRevenue.Value != estimateRevenue.Value)
        //        {
        //            decimal minimumThreshold = 10000;
        //            decimal estimateValue = estimateRevenue.Value;

        //            if (estimateValue < minimumThreshold)
        //            {
        //                // Retrieve the ID of the sales manager user
        //                Guid salesManagerUserId = GetSalesManagerUserId(service, tracingService);

        //                // Assign the opportunity record to the sales manager user
        //                EntityReference salesManagerUserRef = new EntityReference("systemuser", salesManagerUserId);
        //                opportunityEntity.Attributes["ownerid"] = salesManagerUserRef;

        //                // Update the "Budget Status" field to "Pending Approval"
        //                opportunityEntity.Attributes["new_BudgetStatus"] = new OptionSetValue(0);
        //            }
        //            else
        //            {
        //                // Update the "Budget Status" field to "Approved"
        //                opportunityEntity.Attributes["new_BudgetStatus"] = new OptionSetValue(1);
        //            }
        //        }
        //    }
        //}
        private void HandleOpportunityUpdate(IServiceProvider serviceProvider, ITracingService tracingService, IPluginExecutionContext context)
        {
            // Start tracing
            tracingService.Trace("Handling opportunity update...");

            Entity opportunityEntity = (Entity)context.InputParameters["Target"];

            if (opportunityEntity != null)
            {
                tracingService.Trace("Opportunity entity is not null.");

                if (opportunityEntity.Contains("estimatedvalue"))
                {
                    tracingService.Trace("Opportunity entity contains estimated value field.");

                    Money preImageEstimateRevenue = null;
                    if (context.PreEntityImages.Contains("PreImage"))
                    {
                        preImageEstimateRevenue = (Money)context.PreEntityImages["PreImage"]["estimatedvalue"];
                        tracingService.Trace("Pre-Image contains estimated value field.");
                    }
                    else
                    {
                        tracingService.Trace("Pre-Image does not contain estimated value field.");
                    }

                    Money estimateRevenue = (Money)opportunityEntity.Attributes["estimatedvalue"];

                    // Check if estimate revenue has changed
                    if (preImageEstimateRevenue == null || preImageEstimateRevenue.Value != estimateRevenue.Value)
                    {
                        tracingService.Trace("Estimate revenue has changed or Pre-Image is not available.");

                        decimal minimumThreshold = 10000;
                        decimal estimateValue = estimateRevenue.Value;

                        if (estimateValue < minimumThreshold)
                        {
                            tracingService.Trace("Estimate value is less than minimum threshold.");

                            // Retrieve the service factory to create organization service
                            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                            // Create an instance of the organization service
                            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                            // Retrieve the ID of the sales manager user
                            Guid salesManagerUserId = GetSalesManagerUserId(service, tracingService);

                            // Assign the opportunity record to the sales manager user
                            EntityReference salesManagerUserRef = new EntityReference("systemuser", salesManagerUserId);
                            opportunityEntity.Attributes["ownerid"] = salesManagerUserRef;

                            // Update the "Budget Status" field to "Pending Approval"
                            opportunityEntity.Attributes["new_BudgetStatus"] = new OptionSetValue(0);
                        }
                        else
                        {
                            tracingService.Trace("Estimate value is equal or greater than minimum threshold.");

                            // Update the "Budget Status" field to "Approved"
                            opportunityEntity.Attributes["new_BudgetStatus"] = new OptionSetValue(1);
                        }
                    }
                    else
                    {
                        tracingService.Trace("Estimate revenue has not changed.");
                    }
                }
                else
                {
                    tracingService.Trace("Opportunity entity does not contain estimated value field.");
                }
            }
            else
            {
                tracingService.Trace("Opportunity entity is null.");
            }
        }


        // Method to retrieve the ID of the sales manager user
        private Guid GetSalesManagerUserId(IOrganizationService service, ITracingService tracingService)
        {
            // Start tracing
            tracingService.Trace("GetSalesManagerUserId method started...");

            // Example: You might have a custom entity named "Sales Manager" where you store the information about the sales manager user
            QueryExpression query = new QueryExpression("systemuser");
            query.ColumnSet = new ColumnSet("systemuserid");

            // Execute the query to retrieve the sales manager user
            EntityCollection salesManagerUsers = service.RetrieveMultiple(query);

            if (salesManagerUsers.Entities.Count > 0)
            {
                // For simplicity, assume the first user found is the sales manager
                Guid salesManagerUserId = salesManagerUsers.Entities[0].GetAttributeValue<Guid>("systemuserid");

                // Trace success
                tracingService.Trace("Sales manager user found: {0}", salesManagerUserId);

                // End tracing
                tracingService.Trace("GetSalesManagerUserId method completed.");

                return salesManagerUserId;
            }
            else
            {
                // Handle the case where no sales manager user is found
                tracingService.Trace("Sales manager user not found.");

                // End tracing
                tracingService.Trace("GetSalesManagerUserId method completed.");

                // Return a default value or handle the situation according to your application logic
                return Guid.Empty;
            }
        }
    }
}



//using Microsoft.Xrm.Sdk;
//using Microsoft.Xrm.Sdk.Query;
//using System;

//namespace OpportunityApproval
//{
//    public class OpportunityApproval : IPlugin
//    {
//        public void Execute(IServiceProvider serviceProvider)
//        {
//            // Retrieve the context of the plugin execution
//            IPluginExecutionContext context = serviceProvider.GetService(typeof(IPluginExecutionContext)) as IPluginExecutionContext;

//            // Retrieve the tracing service
//            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

//            // Start tracing
//            tracingService.Trace("OpportunityApproval plugin started...");

//            try
//            {
//                // Retrieve the service factory to create organization service
//                IOrganizationServiceFactory serviceFactory = serviceProvider.GetService(typeof(IOrganizationServiceFactory)) as IOrganizationServiceFactory;

//                // Create an instance of the organization service
//                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

//                Entity opportunityEntity = null;
//                // Check if the plugin was called with an entity as the target
//                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
//                {
//                    // Retrieve the entity on which the plugin is operating
//                    opportunityEntity = (Entity)context.InputParameters["Target"];
//                }
//                //else if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
//                //{
//                //    // If it's an update request, retrieve the entity from the database
//                //    EntityReference entityRef = (EntityReference)context.InputParameters["Target"];
//                //    opportunityEntity = service.Retrieve(entityRef.LogicalName, entityRef.Id, new ColumnSet("estimatedvalue", "new_BudgetStatus"));
//                //}

//                // Plugin requirement: If the estimate revenue is less than the minimum threshold of 10000, assign a sales manager for approval. If it's equal to or greater than 10000, update the field and save the record.
//                if (opportunityEntity != null && opportunityEntity.Contains("estimatedvalue"))
//                {
//                    decimal minimumThreshold = 10000;
//                    Money estimateRevenue = (Money)opportunityEntity.Attributes["estimatedvalue"];
//                    decimal EstimateValue = estimateRevenue.Value;

//                    if (EstimateValue < minimumThreshold)
//                    {
//                        // Retrieve the ID of the sales manager user
//                        Guid salesManagerUserId = GetSalesManagerUserId(service, tracingService); // Pass the service instance as a parameter

//                        // Assign the opportunity record to the sales manager user
//                        EntityReference salesManagerUserRef = new EntityReference("systemuser", salesManagerUserId);
//                        // Assign the opportunity record to the sales manager user
//                        //EntityReference salesManagerUserRef = new EntityReference("systemuser", new Guid("32371dd9-2b02-ef11-9f89-000d3a0943af"));
//                        opportunityEntity.Attributes["ownerid"] = salesManagerUserRef;

//                        // Update the "Budget Status" field to "Pending Approval"
//                        opportunityEntity.Attributes["new_BudgetStatus"] = new OptionSetValue(0);

//                        // Save the changes to the record
//                        //service.Update(opportunityEntity);

//                        // Trace success
//                        tracingService.Trace("Opportunity assigned to sales manager for approval.");
//                    }
//                    else
//                    {
//                        // Update the "Budget Status" field to "Approved"
//                        opportunityEntity.Attributes["new_BudgetStatus"] = new OptionSetValue(1);

//                        // Save the changes to the record
//                        //service.Update(opportunityEntity);

//                        // Trace success
//                        tracingService.Trace("Opportunity approved.");
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                // Log any exceptions
//                tracingService.Trace("An error occurred: {0}", ex.ToString());
//                throw;
//            }

//            // End tracing
//            tracingService.Trace("OpportunityApproval plugin completed.");
//        }
//        // Method to retrieve the ID of the sales manager user
//        private Guid GetSalesManagerUserId(IOrganizationService service, ITracingService tracingService)
//        {
//            // Start tracing
//            tracingService.Trace("GetSalesManagerUserId method started...");

//            // Example: You might have a custom entity named "Sales Manager" where you store the information about the sales manager user
//            QueryExpression query = new QueryExpression("systemuser");
//            query.ColumnSet = new ColumnSet("systemuserid");

//            // Execute the query to retrieve the sales manager user
//            EntityCollection salesManagerUsers = service.RetrieveMultiple(query);

//            if (salesManagerUsers.Entities.Count > 0)
//            {
//                // For simplicity, assume the first user found is the sales manager
//                Guid salesManagerUserId = salesManagerUsers.Entities[0].GetAttributeValue<Guid>("systemuserid");

//                // Trace success
//                tracingService.Trace("Sales manager user found: {0}", salesManagerUserId);

//                // End tracing
//                tracingService.Trace("GetSalesManagerUserId method completed.");

//                return salesManagerUserId;
//            }
//            else
//            {
//                // Handle the case where no sales manager user is found
//                tracingService.Trace("Sales manager user not found.");

//                // End tracing
//                tracingService.Trace("GetSalesManagerUserId method completed.");

//                // Return a default value or handle the situation according to your application logic
//                return Guid.Empty;
//            }
//        }
//    }
//}

