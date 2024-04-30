using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace InvoiceGenerationPlugin
{
    public class InvoiceGenerationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            // Check if the plugin is triggered by an update message on the sales order entity or payment status entity.
            if (context.MessageName.ToLower() == "update" &&
                (context.PrimaryEntityName.ToLower() == "salesorder" || context.PrimaryEntityName.ToLower() == "paymentstatus"))
            {
                Entity targetEntity = null;
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {
                    targetEntity = (Entity)context.InputParameters["Target"];
                }
                else
                {
                    return;
                }

                // Check if the order is fulfilled or payment status is complete.
                bool orderFulfilled = IsOrderFulfilled(service, targetEntity);
                bool paymentComplete = IsPaymentComplete(service, targetEntity);

                if (orderFulfilled || paymentComplete)
                {
                    // Get order and customer information.
                    Entity order = GetOrderDetails(service, targetEntity);
                    Entity customer = GetCustomerDetails(service, order);

                    // Generate invoice based on order details and customer information.
                    Entity invoice = GenerateInvoice(service, order, customer);

                    // Update invoice status.
                    UpdateInvoiceStatus(service, invoice);

                    // Track payment due dates and send reminders.
                    TrackPaymentsAndReminders(service, invoice, customer);
                }
            }
        }

        private bool IsOrderFulfilled(IOrganizationService service, Entity targetEntity)
        {
            // Add logic to check if the order is fulfilled. For example, check if the status is "Fulfilled".
            if (targetEntity.LogicalName == "salesorder" && targetEntity.Attributes.Contains("statuscode"))
            {
                OptionSetValue status = (OptionSetValue)targetEntity.Attributes["statuscode"];
                if (status.Value == 3) // Assuming status code 3 represents "Fulfilled"
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsPaymentComplete(IOrganizationService service, Entity targetEntity)
        {
            // Add logic to check if the payment status is complete. For example, check if the status is "Complete".
            if (targetEntity.LogicalName == "paymentstatus" && targetEntity.Attributes.Contains("statuscode"))
            {
                OptionSetValue status = (OptionSetValue)targetEntity.Attributes["statuscode"];
                if (status.Value == 2) // Assuming status code 2 represents "Complete"
                {
                    return true;
                }
            }
            return false;
        }

        private Entity GetOrderDetails(IOrganizationService service, Entity targetEntity)
        {
            // Retrieve order details based on the target entity.
            if (targetEntity.LogicalName == "salesorder")
            {
                return targetEntity;
            }
            else if (targetEntity.LogicalName == "paymentstatus" && targetEntity.Attributes.Contains("salesorderid"))
            {
                Guid orderId = ((EntityReference)targetEntity.Attributes["salesorderid"]).Id;
                return service.Retrieve("salesorder", orderId, new ColumnSet(true));
            }
            return null;
        }

        private Entity GetCustomerDetails(IOrganizationService service, Entity order)
        {
            // Retrieve customer details based on the order.
            if (order.Attributes.Contains("customerid"))
            {
                EntityReference customerRef = (EntityReference)order.Attributes["customerid"];
                return service.Retrieve(customerRef.LogicalName, customerRef.Id, new ColumnSet(true));
            }
            return null;
        }

        private Entity GenerateInvoice(IOrganizationService service, Entity order, Entity customer)
        {
            // Add logic to generate an invoice based on order details and customer information.
            // For demonstration purposes, let's assume we create a new invoice entity and populate it with relevant information.
            Entity invoice = new Entity("invoice");
            invoice["name"] = "Invoice for Order: " + order["name"];
            invoice["customerid"] = new EntityReference(customer.LogicalName, customer.Id);
            invoice["salesorderid"] = new EntityReference(order.LogicalName, order.Id);
            // Add more attributes as needed

            return invoice;
        }

        private void UpdateInvoiceStatus(IOrganizationService service, Entity invoice)
        {
            // Add logic to update the invoice status. For example, set status to "Open".
            invoice["statuscode"] = new OptionSetValue(1); // Assuming status code 1 represents "Open"
            service.Update(invoice);
        }

        private void TrackPaymentsAndReminders(IOrganizationService service, Entity invoice, Entity customer)
        {
            // Add logic to track payment due dates and send reminders.
            // For demonstration purposes, let's assume we set a payment due date and schedule reminders.
            DateTime dueDate = DateTime.Now.AddDays(30); // Set payment due date to 30 days from now
            invoice["duedate"] = dueDate;
            service.Update(invoice);

            // Send reminder to customer for overdue payment (example reminder logic)
            if (dueDate < DateTime.Now)
            {
                // Logic to send reminder to customer
                // For example, send an email
                SendReminderEmail(service, customer, invoice);
            }
        }

        private void SendReminderEmail(IOrganizationService service, Entity customer, Entity invoice)
        {
            // Add logic to send reminder email to customer.
            // For demonstration purposes, let's assume we send an email using Dynamics 365 email functionality.
            Entity email = new Entity("email");
            email["to"] = new EntityCollection(new Entity[] { new Entity(customer.LogicalName, customer.Id) }); // Corrected line
            email["subject"] = "Reminder: Payment Due for Invoice #" + invoice["name"];
            email["description"] = "Please make the payment for the invoice #" + invoice["name"] + " at your earliest convenience.";
            // Set additional email attributes
            service.Create(email);
        }
    }
}

//using Microsoft.Xrm.Sdk;
//using System;

//namespace InvoiceGenerationPlugin
//{
//    public class InvoiceGenerationPlugin : IPlugin
//    {
//        public void Execute(IServiceProvider serviceProvider)
//        {
//            // Obtain the execution context from the service provider.
//            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

//            // Check if the plugin is triggered by an update message on the sales order entity or payment status entity.
//            if (context.MessageName.ToLower() == "update" &&
//                (context.PrimaryEntityName.ToLower() == "salesorder" || context.PrimaryEntityName.ToLower() == "paymentstatus"))
//            {
//                // Check if the order is fulfilled or payment status is complete.
//                bool orderFulfilled = IsOrderFulfilled(context);
//                bool paymentComplete = IsPaymentComplete(context);

//                if (orderFulfilled || paymentComplete)
//                {
//                    // Get order and customer information.
//                    Entity order = (Entity)context.InputParameters["Target"];
//                    Entity customer = GetCustomerDetails(order);

//                    // Generate invoice based on order details and customer information.
//                    Entity invoice = GenerateInvoice(order, customer);

//                    // Update invoice status.
//                    UpdateInvoiceStatus(invoice);

//                    // Track payment due dates and send reminders.
//                    TrackPaymentsAndReminders(invoice);
//                }
//            }
//        }

//        private bool IsOrderFulfilled(IPluginExecutionContext context)
//        {
//            // Add logic to check if the order is fulfilled.
//            throw new NotImplementedException();
//        }

//        private bool IsPaymentComplete(IPluginExecutionContext context)
//        {
//            // Add logic to check if the payment status is complete.
//            throw new NotImplementedException();
//        }

//        private Entity GetCustomerDetails(Entity order)
//        {
//            // Add logic to retrieve customer details based on the order.
//            throw new NotImplementedException();
//        }

//        private Entity GenerateInvoice(Entity order, Entity customer)
//        {
//            // Add logic to generate an invoice based on order details and customer information.
//            throw new NotImplementedException();
//        }

//        private void UpdateInvoiceStatus(Entity invoice)
//        {
//            // Add logic to update the invoice status.
//            throw new NotImplementedException();
//        }

//        private void TrackPaymentsAndReminders(Entity invoice)
//        {
//            // Add logic to track payment due dates and send reminders.
//            throw new NotImplementedException();
//        }
//    }
//}


