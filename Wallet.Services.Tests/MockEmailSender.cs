using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wallet.Services.Contracts;

namespace Wallet.Services.Tests
{
    public class MockEmailSender
    {
        public Mock<IEmailSender> GetMockEmailSender()
        {
            var mockEmailSender = new Mock<IEmailSender>();

            // Setup the SendEmail method
            mockEmailSender.Setup(sender => sender.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                           .Returns(Task.CompletedTask);

            return mockEmailSender;
        }
    }
}
