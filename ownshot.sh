#!/bin/bash
# https://github.com/ardaozkal/ownshot
# based on https://github.com/jomo/imgur-screenshot

# use with https://github.com/ardaozkal/PHP-FileUploader

current_version="v1.3.0"

function is_mac() {
  uname | grep -q "Darwin"
}

############# BASIC CONFIG #############

uploadlink="" # example: https://s.ave.zone/upload.php?apikey=no&rand=yes
open="false" # Open the link? true/false
edit="false" # Edit before uploading? true/false
mode="select" # What should be captured? select/window/full
copy_url="true" # Copy URL after upload? true/false
keep_file="false" # Keep image after uploading? true/false
file_dir="${HOME}/Pictures" # Location for images to be saved.

########### END BASIC CONFIG ###########

########### ADVANCED CONFIG ############

file_name_format="ownshot-%Y_%m_%d-%H:%M:%S.png"

edit_command="gimp %img"

log_file="${HOME}/.ownshot.log"
icon_path="${HOME}/Pictures/ownshot.ico"

upload_connect_timeout="5"
upload_timeout="120"
upload_retries="1"

if is_mac; then
  screenshot_select_command="screencapture -i %img"
  screenshot_window_command="screencapture -iWa %img"
  screenshot_full_command="screencapture %img"
  open_command="open %url"
else
  screenshot_select_command="maim -u -s %img"
  screenshot_window_command="maim -u %img"
  screenshot_full_command="maim -u %img"
  open_command="xdg-open %url"
fi

########## END ADVANCED CONFIG ##########

# dependency check
if [ "${1}" = "--check" ]; then
  (which grep &>/dev/null && echo "OK: found grep") || echo "ERROR: grep not found"
  if is_mac; then
    if which growlnotify &>/dev/null; then
      echo "OK: found growlnotify"
    elif which terminal-notifier &>/dev/null; then
      echo "OK: found terminal-notifier"
    else
      echo "ERROR: growlnotify nor terminal-notifier found"
    fi
    (which screencapture &>/dev/null && echo "OK: found screencapture") || echo "ERROR: screencapture not found"
    (which pbcopy &>/dev/null && echo "OK: found pbcopy") || echo "ERROR: pbcopy not found"
  else
    (which notify-send &>/dev/null && echo "OK: found notify-send") || echo "ERROR: notify-send (from libnotify-bin) not found"
    (which maim &>/dev/null && echo "OK: found maim") || echo "ERROR: maim not found"
    (which slop &>/dev/null && echo "OK: found slop") || echo "ERROR: slop not found"
    (which xclip &>/dev/null && echo "OK: found xclip") || echo "ERROR: xclip not found"
    (which convert &>/dev/null && echo "OK: found imagemagick") || echo "ERROR: imagemagick not found"
  fi
  (which curl &>/dev/null && echo "OK: found curl") || echo "ERROR: curl not found"
  exit 0
fi


# notify <'ok'|'error'> <title> <text>
function notify() {
  if [ -f $icon_path ];
    then
    echo "icon exists, moving on"
    #exists already
  else
    echo "Downloading icon"
    wget "https://raw.githubusercontent.com/aveao/ownshot/master/ownshot/ownshot/bin/Debug/image.ico" --output-document=$icon_path
  fi

  if is_mac; then
    if which growlnotify &>/dev/null; then
      growlnotify  --icon "${icon_path}" --iconpath "${icon_path}" --title "${2}" --message "${3}"
    else
      terminal-notifier -appIcon "${icon_path}" -contentImage "${icon_path}" -title "ownshot: ${2}" -message "${3}"
    fi
  else
    if [ "${1}" = "error" ]; then
      notify-send -a OwnShot -u critical -c "im.error" -i "${icon_path}" -t 5000 "ownshot: ${2}" "${3}"
    else
      notify-send -a OwnShot -u low -c "transfer.complete" -i "/tmp/thumb.png" -t 5000 "ownshot: ${2}" "${3}"
    fi
  fi
}

function take_screenshot() {
  echo "Please select area"
  is_mac || sleep 0.1 # https://bbs.archlinux.org/viewtopic.php?pid=1246173#p1246173

  cmd="screenshot_${mode}_command"
  cmd=${!cmd//\%img/${1}}

  shot_err="$(${cmd} &>/dev/null)" #takes a screenshot with selection
  if [ "${?}" != "0" ]; then
    echo "Failed to take screenshot '${1}': '${shot_err}'. For more information visit https://github.com/jomo/imgur-screenshot/wiki/Troubleshooting" | tee -a "${log_file}" #didn't change link as their troubleshoot likely helps more
    notify error "Something went wrong :(" "Information has been logged"
    exit 1
  fi
  convert -thumbnail 150 ${1} /tmp/thumb.png
}

function upload_image() {
  echo "Uploading '${1}'..."
  title="$(echo "${1}" | rev | cut -d "/" -f 1 | cut -d "." -f 2- | rev)"
  response="$(curl --compressed --connect-timeout "${upload_connect_timeout}" -m "${upload_timeout}" --retry "${upload_retries}" -fsSL --stderr - -F "title=${title}" -F "f=@\"${1}\"" ${uploadlink})"
  handle_upload_success $response "${1}"
}

function handle_upload_success() {
  echo ""
  echo "image link: ${1}"

  if [ "${copy_url}" = "true" ] && [ -z "${album_title}" ]; then
    if is_mac; then
      echo -n "${1}" | pbcopy
    else
      echo -n "${1}" | xclip -selection clipboard
    fi
    echo "URL copied to clipboard"
  fi

  # print to log file: image link, image location, delete link
  echo -e "${1}\t${2}" >> "${log_file}"

  notify ok "Upload done!" "${1}"

  if [ ! -z "${open_command}" ] && [ "${open}" = "true" ]; then
    open_cmd=${open_command//\%url/${1}}
    open_cmd=${open_cmd//\%img/${2}}
    echo "Opening '${open_cmd}'"
    eval "${open_cmd}"
  fi
}

function handle_upload_error() {
  error="Upload failed: \"${1}\""
  echo "${error}"
  echo -e "Error\t${2}\t${error}" >> "${log_file}"
  notify error "Upload failed :(" "${1}"
}

while [ ${#} != 0 ]; do
  case "${1}" in
    -h | --help)
echo "usage: ${0} [-c | --check | -v | -h]"
echo "       ${0} [option]... [file]..."
echo ""
echo "  -h, --help                   Show this help, exit"
echo "  -v, --version                Show current version, exit"
echo "      --check                  Check if all dependencies are installed, exit"
echo "  -o, --open <true|false>      Override 'open' config"
echo "  -e, --edit <true|false>      Override 'edit' config"
echo "  -i, --edit-command <command> Override 'edit_command' config (include '%img'), sets --edit 'true'"
echo "  -k, --keep-file <true|false> Override 'keep_file' config"
echo "  file                         Upload file instead of taking a screenshot"
exit 0;;
-v | --version)
echo "${current_version}"
exit 0;;
-s | --select)
mode="select"
shift;;
-w | --window)
mode="window"
shift;;
-f | --full)
mode="full"
shift;;
-o | --open)
open="${2}"
shift 2;;
-e | --edit)
edit="${2}"
shift 2;;
-i | --edit-command)
edit_command="${2}"
edit="true"
shift 2;;
-k | --keep-file)
keep_file="${2}"
shift 2;;
*)
upload_files=("${@}")
break;;
esac
done

if [ -z "${upload_files}" ]; then
  upload_files[0]=""
fi

for upload_file in "${upload_files[@]}"; do

  if [ -z "${upload_file}" ]; then
    cd "${file_dir}" || exit 1

    # new filename with date
    img_file="$(date +"${file_name_format}")"
    take_screenshot "${img_file}"
  else
    # upload file instead of screenshot
    img_file="${upload_file}"
  fi

  # get full path
  img_file="$(cd "$( dirname "${img_file}")" && echo "$(pwd)/$(basename "${img_file}")")"

  # check if file exists
  if [ ! -f "${img_file}" ]; then
    echo "file '${img_file}' doesn't exist !"
    exit 1
  fi

  # open image in editor if configured
  if [ "${edit}" = "true" ]; then
    edit_cmd=${edit_command//\%img/${img_file}}
    echo "Opening editor '${edit_cmd}'"
    if ! (eval "${edit_cmd}"); then
      echo "Error for image '${img_file}': command '${edit_cmd}' failed, not uploading. For more information visit https://github.com/jomo/imgur-screenshot/wiki/Troubleshooting" | tee -a "${log_file}"
      notify error "Something went wrong :(" "Information has been logged"
      exit 1
    fi
  fi

  upload_image "${img_file}"

  # delete file if configured
  if [ "${keep_file}" = "false" ] && [ -z "${1}" ]; then
    echo "Deleting temp file ${file_dir}/${img_file}"
    rm -rf "${img_file}"
  fi

  echo ""
done
