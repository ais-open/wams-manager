document.write(
    "<div class='navbar'>" +
        "<div class='navbar-inner'>" +
            "<div class='container'>" +
                "<button type='button' class='btn btn-navbar' data-toggle='collapse' data-target='.nav-collapse'>" +
                    "<span class='icon-bar'></span>" +
                    "<span class='icon-bar'></span>" +
                    "<span class='icon-bar'></span>" +
                "</button>" +
                "<img style='float: right; height: 60px; margin: 5px;' src='../Images/logo.png' alt='Applied Info Sciences' />" +
                "<div class='brand'>WAMS tool Management Portal</div>" +
                "<div class='nav-collapse collapse'>" +
                    "<ul class='nav'>" +
                        "<li id='homeLink'><a href='./Home.html'>Home</a></li>" +
                        "<li id='mediaLink' ><a href='./MediaService.html'>Media Services</a></li>" +
                        "<li id='encodingLink'><a href='./Encoding.html'>Encoding</a></li>" +
                       // "<li ><a href='./About.html'>About</a></li>" +
                    "</ul>" +

                    "<div class='pull-right'>" +
                        "<ul class='nav pull-right'>" +
                        "<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown'>Welcome, <span id='loggedUser'></span><b class='caret'></b></a>" +
                         "<ul class='dropdown-menu'>" +
                        "<li><a href='./ChangePassword.html'><i class='icon-cog' style='font-size: 17px;'></i> Change Password</a></li>" +
                        "<li id='createUserLink'><a href='../UserPages/CreateUser.html'><i id='I1' class='icon-user' style='padding: 0px;'></i><i id='I1' class='icon-plus' style='margin: -2px;font-size: 10px;'></i> Create User</a></li>" +
                        "<li id='manageUserLink'><a href='../UserPages/ManageUser.html' style=''><i class='icon-user'></i><i style='font-size: 11px;margin: -2px;' class='icon-cog'></i> Manage Users</a></li><li class='divider'></li>" +
                        "<li><a id='signout' href='#' style=''><i class='icon-off' style='font-size: 17px;'></i> Logout</a></li>" +
                        "</ul>" +
                          "</li>" +
                          "</ul>" +
                    "</div>" +
                "</div>" +

            "</div>" +
        "</div>" +
    "</div>");