/*
* Copyright (C) 2009-2012 Solmead Productions
*
* == BEGIN LICENSE ==
*
* Licensed under the terms of any of the following licenses at your
* choice:
*
*  - GNU General Public License Version 2 or later (the "GPL")
*    http://www.gnu.org/licenses/gpl.html
*
*  - GNU Lesser General Public License Version 2.1 or later (the "LGPL")
*    http://www.gnu.org/licenses/lgpl.html
*
*  - Mozilla Public License Version 1.1 or later (the "MPL")
*    http://www.mozilla.org/MPL/MPL-1.1.html
*
* == END LICENSE ==
*/
//window.System = window.System || {};

System = System || {};
System.FileUpload = System.FileUpload || {};
System.FileUpload.Flash = System.FileUpload.Flash || {};

System.FileUploaders = [];

System.FileUpload.Flash.scriptFilename = 'FlashUpload.js'; // don't forget to set the filename 
System.FileUpload.Flash.scriptUrl = (function () {
    if (document.currentScript) { // support defer & async (mozilla only)
        return document.currentScript.src;
    } else {
        var ls, s;
        var getSrc = function (ls, attr) {
            var i, l = ls.length, nf, s;
            for (i = 0; i < l; i++) {
                s = null;
                if (ls[i].getAttribute.length !== undefined) {
                    s = ls[i].getAttribute(attr, 2);
                }
                if (!s) continue; // tag with no src
                nf = s;
                nf = nf.split('?')[0].split('/').pop(); // get script filename
                if (nf === System.FileUpload.Flash.scriptFilename) {
                    return s;
                }
            }
        };
        ls = document.getElementsByTagName('script');
        s = getSrc(ls, 'src');
        if (!s) { // search reference of script loaded by jQuery.getScript() in meta[name=srcipt][content=url]
            ls = document.getElementsByTagName('meta');
            s = getSrc(ls, 'content');
        }
        if (s) return s;
    }
    return '';
})();

System.FileUpload.Flash.scriptPath = System.FileUpload.Flash.scriptUrl.substring(0, System.FileUpload.Flash.scriptUrl.lastIndexOf('/')) + "/";

System.FileUpload.Flash.CreateFileUploader = function (Area, FileGUID, initializedCallBack, finishedCallBack) {
	var obj = {};

	var fileGUID = "not_set";
	var callbackWhenFinished = null;
	var BaseArea = null;

	formatUnits = function (baseNumber, unitDivisors, unitLabels, singleFractional) {
		var i, unit, unitDivisor, unitLabel;

		if (baseNumber === 0) {
			return "0 " + unitLabels[unitLabels.length - 1];
		}

		if (singleFractional) {
			unit = baseNumber;
			unitLabel = unitLabels.length >= unitDivisors.length ? unitLabels[unitDivisors.length - 1] : "";
			for (i = 0; i < unitDivisors.length; i++) {
				if (baseNumber >= unitDivisors[i]) {
					unit = (baseNumber / unitDivisors[i]).toFixed(2);
					unitLabel = unitLabels.length >= i ? " " + unitLabels[i] : "";
					break;
				}
			}

			return unit + unitLabel;
		} else {
			var formattedStrings = [];
			var remainder = baseNumber;

			for (i = 0; i < unitDivisors.length; i++) {
				unitDivisor = unitDivisors[i];
				unitLabel = unitLabels.length > i ? " " + unitLabels[i] : "";

				unit = remainder / unitDivisor;
				if (i < unitDivisors.length - 1) {
					unit = Math.floor(unit);
				} else {
					unit = unit.toFixed(2);
				}
				if (unit > 0) {
					remainder = remainder % unitDivisor;

					formattedStrings.push(unit + unitLabel);
				}
			}

			return formattedStrings.join(" ");
		}
	};
	formatBPS = function (baseNumber) {
		var bpsUnits = [1073741824, 1048576, 1024, 1], bpsUnitLabels = ["Gbps", "Mbps", "Kbps", "bps"];
		return formatUnits(baseNumber, bpsUnits, bpsUnitLabels, true);

	};
	formatTime = function (baseNumber) {
		var timeUnits = [86400, 3600, 60, 1], timeUnitLabels = ["d", "h", "m", "s"];
		return formatUnits(baseNumber, timeUnits, timeUnitLabels, false);

	};
	formatBytes = function (baseNumber) {
		var sizeUnits = [1073741824, 1048576, 1024, 1], sizeUnitLabels = ["GB", "MB", "KB", "bytes"];
		return formatUnits(baseNumber, sizeUnits, sizeUnitLabels, true);

	};
	formatPercent = function (baseNumber) {
		return baseNumber.toFixed(2) + " %";
	};

	var uploaderIndex = 0;
	var slCtl = null;

	obj = {
		areaID: null,
		init: function (area, initializedCallBack, finishedcallback) {
			var id = $(area).attr("id");
			obj.areaID = id;
			fileGUID = FileGUID;
			callbackWhenFinished = finishedcallback;
			System.FileUploaders.push(obj);
			uploaderIndex = System.FileUploaders.length - 1;
			obj.CreateUploader(area);
			slCtl = document.getElementById(id);
			//            var it = System.FileUploaders[uploaderIndex];
			//            
			//            var i = 0;
			
		},
		StopUpload: function () {
			slCtl.Content.Page.StopUpload();
		},
		ShowDialog: function () {
			slCtl.Content.Page.ShowDialog();
		},
		CreateUploader: function (position) {
			BaseArea = $(position);
			var id = $(BaseArea).attr("id");
		    var parent = $(BaseArea).parent();
		    
		    obj.CreateSWFObject();

		    $(parent).append($("<input type='hidden' id='" + id + "' name='" + id + "'/>"));
		    BaseArea = $("#" + id);


		},
		Loaded: function () {
		    var id = $(BaseArea).attr("id");

		    $(BaseArea).before($("<div id='" + id + "_completedMessage' style='display:none; float:left;'/>"));
		    $(BaseArea).before($("<input type='hidden' id='" + id + "_filename' name='" + id + "_filename'/>"));
		    $(BaseArea).before($("<input type='hidden' id='" + id + "_guid' name='" + id + "_guid'/>"));



		    obj.CreateDisplayArea();
		    $(BaseArea).hide();
		    $("#" + id + "progress-bar").hide();
		    $("#" + id + "_uploading").hide();

		    $("#" + id + "_uploaderarea").dialog({
		        autoOpen: false,
		        bgiframe: true,
		        height: 140,
		        width: 400,
		        modal: false
		    });

		    if (initializedCallBack) {
		        initializedCallBack();
		    }
		},
		SilverLightLoaded: function (sender, args) {
			slCtl = sender.getHost();
		},
		CreateSWFObject: function () {
			var id = $(BaseArea).attr("id");
			$(BaseArea).parent().swfupload({
			    upload_url: "FileHandler.axd?FileGUID=" + fileGUID,
				file_size_limit: "1024000",
				file_types: "*.*",
				file_types_description: "All Files",
				file_upload_limit: "0",
				flash_url: "/content/flash/swfupload/swfupload.swf",
				button_image_url: '/content/flash/swfupload/XPButtonUploadText_61x22.png',
				button_width: 61,
				button_height: 22,
				button_placeholder: $(BaseArea)[0],
				button_placeholder_id: id,
				debug: false,
				custom_settings: { something: "here" }
			})
			.bind('swfuploadLoaded', function (event) {
			    System.DebugWrite('Loaded');
			    obj.Loaded();
			})
			.bind('fileQueued', function (event, file) {
				System.DebugWrite('File queued - ' + file.name + '');
				// start the upload since it's queued
				$(this).swfupload('startUpload');
			})
			.bind('fileQueueError', function (event, file, errorCode, message) {
				System.DebugWrite('File queue error - ' + message + '');
			})
			.bind('fileDialogStart', function (event) {
			    System.DebugWrite('File dialog start');
			})
			.bind('fileDialogComplete', function (event, numFilesSelected, numFilesQueued) {
				System.DebugWrite('File dialog complete');
			})
			.bind('uploadStart', function (event, file) {
				System.DebugWrite('Upload start - ' + file.name + '');
				obj.SilverStart(file);
			})
			.bind('uploadProgress', function (event, file, bytesLoaded) {
			    System.DebugWrite('Upload progress - ' + bytesLoaded + '');
			    var stats = $.swfupload.getInstance(this).getStats();
				obj.SilverUpdate(file, bytesLoaded, stats);
			})
			.bind('uploadSuccess', function (event, file, serverData) {
				System.DebugWrite('Upload success - ' + file.name + '');
			})
			.bind('uploadComplete', function (event, file) {
				System.DebugWrite('Upload complete - ' + file.name + '');
				// upload has completed, lets try the next one in the queue
				//$(this).swfupload('startUpload');
				var stats = $.swfupload.getInstance(this).getStats();
				//console.debug(stats);
				if (stats.files_queued == 0) {
					obj.SilverComplete(file, stats);
				}
			})
			.bind('uploadError', function (event, file, errorCode, message) {
			    System.DebugWrite('Upload error - ' + message + '');
			});


			//var args = "";
			//args = "UploadChunkSize=-1,MaximumUpload=-1,Filter=" + "All Files (*.*)|*.*" + ",JavascriptCompleteFunction=" + ("SilverComplete") + ",FileGUID=" + fileGUID + ",JavascriptStartFunction=" + ("SilverStart") + ",JavascriptStatFunction=" + ("SilverUpdate") + ",ID=" + id + ",JavascriptMessageFunction=SilverMessage";

			//var silver = '    <object ID="' + id + '" data="data:application/x-silverlight-2," type="application/x-silverlight-2" width="105px" height="50px" style="">';
			//silver = silver + '<param name="source" value="/ClientBin/FileUploader.xap"/>';
			//silver = silver + '<param name="onError" value="System.onSilverlightError" />';
			//silver = silver + '<param name="windowless" value="true" />';
			//silver = silver + '<param name="background" value="Transparent" />';
			//silver = silver + '<param name="minRuntimeVersion" value="5.0.61118.0" />';
			//silver = silver + '<param name="autoUpgrade" value="true" />';
			//silver = silver + '<param name="initParams" value="' + args + '" />';
			//silver = silver + '<param name="onLoad" value="System.FileUploaders[' + uploaderIndex + '].SilverLightLoaded" />';
			//silver = silver + '<a href="http://go.microsoft.com/fwlink/?LinkID=149156&v=5.0.61118.0" style="text-decoration:none">';
			//silver = silver + '  <img src="http://go.microsoft.com/fwlink/?LinkId=161376" alt="Get Microsoft Silverlight" style="border-style:none"/>';
			//silver = silver + '</a>';
			//silver = silver + ' </object><iframe id="_sl_historyFrame" style="visibility:hidden;height:0px;width:0px;border:0px"></iframe>';


			//$(BaseArea).before($("<div id='" + id + "_swf' style='float:left;'>" + silver + "</div>"));
		},
		CreateDisplayArea: function () {
			var id = $(BaseArea).attr("id");
			var container = $("<div id='" + id + "_uploaderarea' class='async-uploader'/>");
			container.append($("<div id='" + id + "progress-bar' style='display:none;'><div style='background:#ff0000;'>&nbsp;</div></div>"));
			container.append($("<div id='" + id + "_uploading' style='display:none;'>Uploading... <input type='button' value='Cancel' class='button' style='float:right; display:block;' />" +
				"<div style='display:none;'><span>Current Rate: </span><span id='" + id + "_MovingAverageSpeed'/></div>" +
				"<div style='display:block;'><span>Time Remaining: </span><span id='" + id + "_TimeRemaining'/></div>" +
				"<div style='display:none;'><span>Time Elapsed: </span><span id='" + id + "_TimeElapsed'></span></div>" +
				"<div style='display:none;'><span>Percent Uploaded: </span><span id='" + id + "_PercentUploaded'></span></div>" +
				"<div style='display:block;'><span>Amount of File uploaded: </span><span id='" + id + "_SizeUploaded'/></div>" +
				+"</div>"));
			$(BaseArea).before(container);
		},
		SilverStart: function (UI) {
		    var id = $(BaseArea).attr("id");
			System.DebugWrite("SilverStart");
			$("#" + id + "_filename").val("");
			$("#" + id + "_guid").val("");
			$("#" + id + "progress-bar div").css("width", "0px");
			$("#" + id + "progress-bar").show();
			$("#" + id + "_uploading").show();
			$("#" + id + "_completedMessage").html("").hide();
			$("#" + id + "_uploaderarea").dialog("open");
			$('input[type="submit"]').attr("disabled", "disabled");
		},
		SilverComplete: function (UI) {
		    System.DebugWrite("SilverComplete");
		    var id = $(BaseArea).attr("id");

			var file = UI;
			$("#" + id + "_filename").val(file.name);
			$("#" + id + "_guid").val(fileGUID);
			$("#" + id + "_completedMessage").html("<span class='bright'>Success!</span> Uploaded <b>{0}</b> ({1} KB)."
					.replace("{0}", file.name)
					.replace("{1}", Math.round(file.size / 1024))
			);
			//alert("Complete");
			$("#" + id + "_uploaderarea").dialog("close");
			var clearup = function () {
			    $("#" + id + "progress-bar").hide();
			    $("#" + id + "_completedMessage").show();
			    $("#" + id + "_uploading").hide();
			};
			if ($("#" + id + "_filename").val() != "") // Success
			    $("#" + id + "progress-bar div").animate({ width: "100%" }, { duration: "fast", queue: false, complete: clearup });
			else // Fail
				clearup();

			$('input[type="submit"]').removeAttr("disabled");
			if (callbackWhenFinished) {
				callbackWhenFinished();
			}
		},
		SilverUpdate: function (UI, bytesUploaded, stats) {
		    var id = $(BaseArea).attr("id");
		    var tdMovingAverageSpeed = document.getElementById("" + id + "_MovingAverageSpeed");
		    var tdTimeRemaining = document.getElementById("" + id + "_TimeRemaining");
		    var tdTimeElapsed = document.getElementById("" + id + "_TimeElapsed");
		    var tdPercentUploaded = document.getElementById("" + id + "_PercentUploaded");
		    var tdSizeUploaded = document.getElementById("" + id + "_SizeUploaded");

			var file = UI;
			var percent = 100 * file.sizeUploaded / file.size;
			$("#" + id + "progress-bar div").animate({ width: percent + "%" }, { duration: 500, queue: false });
			//this.customSettings.tdCurrentSpeed.innerHTML = SWFUpload.speed.formatBPS(file.currentSpeed);
			//this.customSettings.tdAverageSpeed.innerHTML = SWFUpload.speed.formatBPS(file.averageSpeed);
			//tdMovingAverageSpeed.innerHTML = formatBPS(file.movingAverageSpeed);
			tdTimeRemaining.innerHTML = formatTime(file.timeRemaining);
			//tdTimeElapsed.innerHTML = formatTime(file.timeElapsed);
			//tdPercentUploaded.innerHTML = formatPercent(file.percentUploaded);
			tdSizeUploaded.innerHTML = formatBytes(file.sizeUploaded);
		},
		SilverMessage: function (UI) {
			System.DebugWrite(UI.Message);
		}
	};

	$(function () {
		obj.init(Area, initializedCallBack, finishedCallBack);
	});

	return obj;
};

function SilverStart(UI) {
	$.each(System.FileUploaders, function (i, item) {
		if (item.areaID == UI.ID) {
			item.SilverStart(UI);
		}
	});
}
function SilverUpdate(UI) {
	$.each(System.FileUploaders, function (i, item) {
		if (item.areaID == UI.ID) {
			item.SilverUpdate(UI);
		}
	});
}
function SilverMessage(UI) {

	System.DebugWrite(UI.Message);
}
function SilverComplete(UI) {
	$.each(System.FileUploaders, function (i, item) {
		if (item.areaID == UI.ID) {
			item.SilverComplete(UI);
		}
	});
}

