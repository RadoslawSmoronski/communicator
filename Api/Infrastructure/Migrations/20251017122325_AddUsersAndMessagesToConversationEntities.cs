using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndMessagesToConversationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendshipInvitations_AspNetUsers_RecipientUserId",
                table: "FriendshipInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversation_ConversationId",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "Conversation");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_FriendshipInvitations_RecipientUserId",
                table: "FriendshipInvitations");

            migrationBuilder.DropColumn(
                name: "RecipientUserId",
                table: "FriendshipInvitations");

            migrationBuilder.AddColumn<Guid>(
                name: "LastMessageId",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMessageTime",
                table: "Conversations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "User1LastReadMessageId",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "User2LastReadMessageId",
                table: "Conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendshipInvitations_RecipientId",
                table: "FriendshipInvitations",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_LastMessageId",
                table: "Conversations",
                column: "LastMessageId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Messages_LastMessageId",
                table: "Conversations",
                column: "LastMessageId",
                principalTable: "Messages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FriendshipInvitations_AspNetUsers_RecipientId",
                table: "FriendshipInvitations",
                column: "RecipientId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversations_ConversationId",
                table: "Messages",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Messages_LastMessageId",
                table: "Conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendshipInvitations_AspNetUsers_RecipientId",
                table: "FriendshipInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Conversations_ConversationId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_FriendshipInvitations_RecipientId",
                table: "FriendshipInvitations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_LastMessageId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "LastMessageId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "LastMessageTime",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "User1LastReadMessageId",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "User2LastReadMessageId",
                table: "Conversations");

            migrationBuilder.AddColumn<Guid>(
                name: "RecipientUserId",
                table: "FriendshipInvitations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Conversation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User1Id = table.Column<Guid>(type: "uuid", nullable: false),
                    User2Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversation_User_User1Id",
                        column: x => x.User1Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversation_User_User2Id",
                        column: x => x.User2Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendshipInvitations_RecipientUserId",
                table: "FriendshipInvitations",
                column: "RecipientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_User1Id",
                table: "Conversation",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Conversation_User2Id",
                table: "Conversation",
                column: "User2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendshipInvitations_AspNetUsers_RecipientUserId",
                table: "FriendshipInvitations",
                column: "RecipientUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_AspNetUsers_SenderId",
                table: "Messages",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Conversation_ConversationId",
                table: "Messages",
                column: "ConversationId",
                principalTable: "Conversation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
